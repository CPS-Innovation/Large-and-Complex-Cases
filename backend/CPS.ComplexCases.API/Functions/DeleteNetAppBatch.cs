using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.API.Functions;

public class DeleteNetAppBatch(
    ILogger<DeleteNetAppBatch> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<DeleteNetAppBatch> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DeleteNetAppBatch))]
    [OpenApiOperation(operationId: nameof(DeleteNetAppBatch), tags: ["NetApp"], Description = "Delete one or more files and folders from NetApp in a single batch request.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(DeleteNetAppBatchDto), Description = "Body containing the case ID and list of files/folders to delete.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(DeleteNetAppBatchResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/delete/batch")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var batchRequest = await _requestValidator.GetJsonBody<DeleteNetAppBatchDto, DeleteNetAppBatchRequestValidator>(req);

        if (!batchRequest.IsValid)
            return new BadRequestObjectResult(batchRequest.ValidationErrors);

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(batchRequest.Value.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
            return new BadRequestObjectResult(new[] { "Case metadata or NetApp folder path is missing." });

        var casePrefix = caseMetadata.NetappFolderPath.EnsureTrailingSlash();

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucket = securityGroups.First().BucketName;

        var results = new List<DeleteNetAppBatchItemResult>();

        foreach (var op in batchRequest.Value.Operations)
        {
            var outOfCaseFailure = CreateOutOfCaseDeleteFailure(op, casePrefix);
            if (outOfCaseFailure is not null)
            {
                _logger.LogWarning(
                    "Path {SourcePath} is not within case folder {CasePrefix}. Skipping.",
                    op.SourcePath, casePrefix);
                results.Add(outOfCaseFailure);
                continue;
            }

            results.Add(await DeleteSingleOperationAsync(op, context.BearerToken, bucket));
        }

        await WriteDeleteActivityLogAsync(batchRequest.Value, results, context.Username);

        return new OkObjectResult(BuildBatchResponse(batchRequest.Value.Operations.Count, results));
    }

    private async Task<DeleteNetAppBatchItemResult> DeleteSingleOperationAsync(
        DeleteNetAppBatchOperationDto op,
        string bearerToken,
        string bucket)
    {
        var isFolder = op.Type == NetAppOperationType.Folder;
        var arg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
            bearerToken, bucket, string.Empty, op.SourcePath, isFolder);

        try
        {
            _logger.LogInformation(
                "Deleting {Type} from NetApp: SourcePath={SourcePath}",
                op.Type, op.SourcePath);

            var result = await _netAppClient.DeleteFileOrFolderAsync(arg);
            return MapDeleteResultToItemResult(op, result, _logger);
        }
        catch (Amazon.S3.AmazonS3Exception ex) when ((int)ex.StatusCode == 423)
        {
            _logger.LogWarning("File {SourcePath} is locked via SMB (HTTP 423).", op.SourcePath);
            return new DeleteNetAppBatchItemResult
            {
                SourcePath = op.SourcePath,
                Status = OperationResultStatus.Failed,
                Error = "File is open via SMB (HTTP 423). Close the file and retry."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting {SourcePath} from bucket {BucketName}.", op.SourcePath, bucket);
            return new DeleteNetAppBatchItemResult
            {
                SourcePath = op.SourcePath,
                Status = OperationResultStatus.Failed,
                Error = ex.Message
            };
        }
    }

    internal static DeleteNetAppBatchItemResult? CreateOutOfCaseDeleteFailure(
        DeleteNetAppBatchOperationDto op,
        string casePrefix)
    {
        if (op.SourcePath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new DeleteNetAppBatchItemResult
        {
            SourcePath = op.SourcePath,
            Status = OperationResultStatus.Failed,
            Error = "Path is not within the case's NetApp folder."
        };
    }

    internal static DeleteNetAppBatchItemResult MapDeleteResultToItemResult(
        DeleteNetAppBatchOperationDto op,
        DeleteNetAppResult result,
        ILogger? logger = null)
    {
        if (!result.Success)
        {
            return new DeleteNetAppBatchItemResult
            {
                SourcePath = op.SourcePath,
                Status = OperationResultStatus.Failed,
                Error = result.ErrorMessage
            };
        }

        if (!result.WasFound)
        {
            logger?.LogInformation("Path {SourcePath} was not found in NetApp; treating as already deleted.", op.SourcePath);
            return new DeleteNetAppBatchItemResult
            {
                SourcePath = op.SourcePath,
                Status = OperationResultStatus.NotFound,
            };
        }

        return new DeleteNetAppBatchItemResult
        {
            SourcePath = op.SourcePath,
            Status = OperationResultStatus.Deleted,
            KeysDeleted = result.KeysDeleted > 1 ? result.KeysDeleted : null
        };
    }

    internal static (ActivityLog.Enums.ActionType ActionType, ActivityLog.Enums.ResourceType ResourceType)
        ResolveDeleteActivityTypes(bool hasFolder, bool hasMaterial) =>
        (hasFolder, hasMaterial) switch
        {
            (true, true) => (ActivityLog.Enums.ActionType.FolderAndMaterialDeleted, ActivityLog.Enums.ResourceType.Material),
            (true, false) => (ActivityLog.Enums.ActionType.FolderDeleted, ActivityLog.Enums.ResourceType.NetAppFolder),
            _ => (ActivityLog.Enums.ActionType.MaterialDeleted, ActivityLog.Enums.ResourceType.Material)
        };

    internal static DeleteNetAppBatchResponse BuildBatchResponse(
        int totalRequested,
        List<DeleteNetAppBatchItemResult> results)
    {
        var succeeded = results.Count(r => r.Status == OperationResultStatus.Deleted);
        var notFound = results.Count(r => r.Status == OperationResultStatus.NotFound);
        var failed = results.Count(r => r.Status == OperationResultStatus.Failed);

        return new DeleteNetAppBatchResponse
        {
            Status = NetAppBatchOutcome.ResolveStatus(succeeded, failed, notFound),
            TotalRequested = totalRequested,
            Succeeded = succeeded,
            NotFound = notFound,
            Failed = failed,
            Results = results
        };
    }

    private async Task WriteDeleteActivityLogAsync(
        DeleteNetAppBatchDto request,
        List<DeleteNetAppBatchItemResult> results,
        string userName)
    {
        var succeeded = results.Count(r => r.Status == OperationResultStatus.Deleted);
        var notFound = results.Count(r => r.Status == OperationResultStatus.NotFound);
        if (succeeded == 0 && notFound == 0)
        {
            return;
        }

        try
        {
            var auditedPaths = results
                .Where(r => r.Status is OperationResultStatus.Deleted or OperationResultStatus.NotFound)
                .Select(r => r.SourcePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var auditedOps = request.Operations
                .Where(op => auditedPaths.Contains(op.SourcePath))
                .ToList();

            var hasFolder = auditedOps.Any(op => op.Type == NetAppOperationType.Folder);
            var hasMaterial = auditedOps.Any(op => op.Type == NetAppOperationType.Material);
            var (actionType, resourceType) = ResolveDeleteActivityTypes(hasFolder, hasMaterial);

            var details = new
            {
                items = results.Select(r => new
                {
                    sourcePath = r.SourcePath,
                    outcome = r.Status,
                    error = r.Error,
                    keysDeleted = r.KeysDeleted
                })
            }.SerializeToJsonDocument(_logger);

            await _activityLogService.CreateActivityLogAsync(
                actionType,
                resourceType,
                request.CaseId,
                request.CaseId.ToString(),
                null,
                userName,
                details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch activity log for case {CaseId}.", request.CaseId);
        }
    }
}

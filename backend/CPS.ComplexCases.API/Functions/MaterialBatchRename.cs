using System.Net;
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
using CPS.ComplexCases.Common.Models.Configuration;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Functions;

public class MaterialBatchRename(
    ILogger<MaterialBatchRename> logger,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator,
    IInitializationHandler initializationHandler,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IOntapArgFactory ontapArgFactory,
    IOntapHttpClient ontapHttpClient,
    IOptions<FeatureFlagConfig> featureFlags)
{
    private readonly ILogger<MaterialBatchRename> _logger = logger;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;
    private readonly IOntapHttpClient _ontapHttpClient = ontapHttpClient;
    private readonly FeatureFlagConfig _featureFlags = featureFlags.Value;

    [Function(nameof(MaterialBatchRename))]
    [OpenApiOperation(operationId: nameof(MaterialBatchRename), tags: ["NetApp"], Description = "Rename one or more files and folders from NetApp in a single batch request.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(MaterialBatchRenameRequestDto), Description = "Body containing the material rename information")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(MaterialRenameBatchResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "v1/netapp/rename/batch")] HttpRequest req, FunctionContext functionContext)
    {
        if (!_featureFlags.MaterialRename)
        {
            return new NotFoundResult();
        }

        var context = functionContext.GetRequestContext();

        var renameRequest = await _requestValidator.GetJsonBody<MaterialBatchRenameRequestDto, MaterialRenameRequestValidator>(req);

        if (!renameRequest.IsValid)
        {
            return new BadRequestObjectResult(renameRequest.ValidationErrors);
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, renameRequest.Value.CaseId);

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(renameRequest.Value.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
            return new BadRequestObjectResult(new[] { "Case metadata or NetApp folder path is missing." });

        var casePrefix = caseMetadata.NetappFolderPath.EnsureTrailingSlash();

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var volumeUuid = securityGroups[0].VolumeUuid;

        var results = new List<MaterialRenameBatchItemResult>();

        foreach (var operation in renameRequest.Value.Operations)
        {
            var outOfCaseFailure = CreateOutOfCaseRenameFailure(operation, casePrefix);
            if (outOfCaseFailure is not null)
            {
                _logger.LogWarning(
                    "Either the current path {CurrentPath} or the new path {NewPath} is not within case folder {CasePrefix}. Skipping.",
                    operation.CurrentPath, operation.NewPath, casePrefix);
                results.Add(outOfCaseFailure);
                continue;
            }

            try
            {
                results.Add(await RenameSingleOperationAsync(operation, context.BearerToken, volumeUuid));
            }
            catch (Exception ex) when (IsAuthException(ex))
            {
                // Items already renamed must be audited before the auth failure propagates.
                await WriteRenameActivityLogAsync(renameRequest.Value, results, context.Username);
                throw;
            }
        }

        await WriteRenameActivityLogAsync(renameRequest.Value, results, context.Username);

        return new OkObjectResult(BuildBatchResponse(renameRequest.Value.Operations.Count, results));
    }

    private async Task<MaterialRenameBatchItemResult> RenameSingleOperationAsync(
        RenameNetAppMaterialBatchOperationDto operation,
        string bearerToken,
        Guid volumeUuid)
    {
        var arg = _ontapArgFactory.CreateMaterialRenameArg(bearerToken, volumeUuid, operation.CurrentPath, operation.NewPath);

        try
        {
            var result = await _ontapHttpClient.RenameMaterialAsync(arg);
            return MapRenameResultToItemResult(operation, result, _logger);
        }
        catch (Exception ex) when (IsAuthException(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error renaming material from {CurrentPath} to {NewPath}.", operation.CurrentPath, operation.NewPath);
            return new MaterialRenameBatchItemResult
            {
                PreviousPath = operation.CurrentPath,
                NewPath = operation.NewPath,
                Status = OperationResultStatus.Failed,
                Error = ex.Message
            };
        }
    }

    internal static MaterialRenameBatchItemResult? CreateOutOfCaseRenameFailure(
        RenameNetAppMaterialBatchOperationDto operation,
        string casePrefix)
    {
        if (operation.CurrentPath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase) &&
            operation.NewPath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new MaterialRenameBatchItemResult
        {
            PreviousPath = operation.CurrentPath,
            NewPath = operation.NewPath,
            Status = OperationResultStatus.Failed,
            Error = "Path is not within the case's NetApp folder."
        };
    }

    internal static MaterialRenameBatchItemResult MapRenameResultToItemResult(
        RenameNetAppMaterialBatchOperationDto operation,
        MaterialRenameResult result,
        ILogger? logger = null)
    {
        if (result.Success)
        {
            return new MaterialRenameBatchItemResult
            {
                PreviousPath = operation.CurrentPath,
                NewPath = operation.NewPath,
                Status = OperationResultStatus.Renamed
            };
        }

        if (!result.WasFound)
        {
            logger?.LogInformation("Material not found for {CurrentPath}.", operation.CurrentPath);
            return new MaterialRenameBatchItemResult
            {
                PreviousPath = operation.CurrentPath,
                NewPath = operation.NewPath,
                Status = OperationResultStatus.NotFound
            };
        }

        logger?.LogInformation(
            "Failed to rename material from {CurrentPath} to {NewPath}. Error: {ErrorMessage}",
            operation.CurrentPath, operation.NewPath, result.ErrorMessage);
        return new MaterialRenameBatchItemResult
        {
            PreviousPath = operation.CurrentPath,
            NewPath = operation.NewPath,
            Status = OperationResultStatus.Failed,
            Error = result.ErrorMessage
        };
    }

    internal static bool IsAuthException(Exception ex) =>
        ex is OntapUnauthorizedException
            or OntapClientException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };

    internal static MaterialRenameBatchResponse BuildBatchResponse(
        int totalRequested,
        List<MaterialRenameBatchItemResult> results)
    {
        var succeeded = results.Count(r => r.Status == OperationResultStatus.Renamed);
        var notFound = results.Count(r => r.Status == OperationResultStatus.NotFound);
        var failed = results.Count(r => r.Status == OperationResultStatus.Failed);

        return new MaterialRenameBatchResponse
        {
            Status = NetAppBatchOutcome.ResolveStatus(succeeded, failed, notFound),
            TotalRequested = totalRequested,
            Succeeded = succeeded,
            NotFound = notFound,
            Failed = failed,
            Results = results
        };
    }

    private async Task WriteRenameActivityLogAsync(
        MaterialBatchRenameRequestDto request,
        List<MaterialRenameBatchItemResult> results,
        string userName)
    {
        var hasAuditableResult = results.Any(r => r.Status is OperationResultStatus.Renamed or OperationResultStatus.NotFound);
        if (!hasAuditableResult)
        {
            return;
        }

        try
        {
            var auditedPaths = results
                .Where(r => r.Status is OperationResultStatus.Renamed or OperationResultStatus.NotFound)
                .Select(r => r.PreviousPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var auditedOps = request.Operations
                .Where(op => auditedPaths.Contains(op.CurrentPath, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var hasFolder = auditedOps.Any(op => op.Type == NetAppOperationType.Folder);
            var hasMaterial = auditedOps.Any(op => op.Type == NetAppOperationType.Material);

            var (actionType, resourceType) = (hasFolder, hasMaterial) switch
            {
                (true, true) => (ActivityLog.Enums.ActionType.FolderAndMaterialRenamed, ActivityLog.Enums.ResourceType.Material),
                (true, false) => (ActivityLog.Enums.ActionType.FolderRenamed, ActivityLog.Enums.ResourceType.NetAppFolder),
                _ => (ActivityLog.Enums.ActionType.MaterialRenamed, ActivityLog.Enums.ResourceType.Material)
            };

            var details = new
            {
                items = results.Select(r => new
                {
                    previousPath = r.PreviousPath,
                    newPath = r.NewPath,
                    outcome = r.Status,
                    error = r.Error
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

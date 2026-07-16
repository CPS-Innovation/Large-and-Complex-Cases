using System.Net;
using System.Text.Json;
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
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.API.Functions;

public class InitiateBatchMove(
    ILogger<InitiateBatchMove> logger,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler,
    IActivityLogService activityLogService,
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    IOntapArgFactory ontapArgFactory,
    IOntapHttpClient ontapHttpClient)
{
    private readonly ILogger<InitiateBatchMove> _logger = logger;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly IOntapArgFactory _ontapArgFactory = ontapArgFactory;
    private readonly IOntapHttpClient _ontapHttpClient = ontapHttpClient;

    [Function(nameof(InitiateBatchMove))]
    [OpenApiOperation(operationId: nameof(InitiateBatchMove), tags: ["NetApp"], Description = "Moves files and folders within NetApp via synchronous ONTAP relocate. Returns per-item results with no transferId.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(MoveNetAppBatchDto), Description = "Body containing the case ID, destination prefix, and list of move operations.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(MoveNetAppBatchResponse), Description = "Move batch completed. Returns per-item outcomes.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/move/batch")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        _logger.LogInformation("{OperationName} request received. CorrelationId: {CorrelationId}", nameof(InitiateBatchMove), context.CorrelationId);

        var batchRequest = await _requestValidator.GetJsonBody<MoveNetAppBatchDto, MoveNetAppBatchRequestValidator>(req);
        if (!batchRequest.IsValid)
        {
            _logger.LogWarning("Validation failed for {OperationName}. CorrelationId: {CorrelationId}, Errors: {Errors}",
                nameof(InitiateBatchMove), context.CorrelationId, batchRequest.ValidationErrors);
            return new BadRequestObjectResult(batchRequest.ValidationErrors);
        }

        var request = batchRequest.Value;
        _initializationHandler.Initialize(context.Username, context.CorrelationId, request.CaseId);

        var pathError = await ValidatePathsAsync(request, context.CorrelationId);
        if (pathError is not null)
        {
            return pathError;
        }

        var destinationPrefix = EnsureTrailingSlash(request.DestinationPrefix);
        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var volumeUuid = securityGroups[0].VolumeUuid;

        var manageMaterialsOperation = CreateManageMaterialsOperation(request, destinationPrefix, context.Username);
        var sourcePaths = request.Operations.Select(op => op.SourcePath).ToList();
        var destinationPaths = new List<string> { destinationPrefix };

        var lockAcquired = await _caseActiveManageMaterialsService.CheckConflictAndInsertAsync(
            manageMaterialsOperation, sourcePaths, destinationPaths);

        if (!lockAcquired)
        {
            _logger.LogWarning("A conflicting manage materials operation is already in progress for case {CaseId}.", request.CaseId);
            return new ConflictObjectResult("A conflicting manage materials operation is already in progress for one or more of the selected paths.");
        }

        var results = new List<MoveNetAppBatchItemResult>();
        try
        {
            try
            {
                await ExecuteMovesAsync(request.Operations, destinationPrefix, context.BearerToken, volumeUuid, results);
            }
            catch (Exception ex) when (IsAuthException(ex))
            {
                // Items already relocated must be audited before the auth failure propagates.
                await WriteMoveActivityLogAsync(request, results, context.Username);
                throw;
            }

            await WriteMoveActivityLogAsync(request, results, context.Username);
            return new OkObjectResult(BuildBatchResponse(request.Operations.Count, results));
        }
        finally
        {
            await ReleaseManageMaterialsLockAsync(manageMaterialsOperation.Id, request.CaseId);
        }
    }

    private async Task<IActionResult?> ValidatePathsAsync(MoveNetAppBatchDto request, Guid correlationId)
    {
        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(request.CaseId);
        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
        {
            _logger.LogWarning("Case metadata or NetApp folder path missing for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                request.CaseId, correlationId);
            return new BadRequestObjectResult(new[] { "Case metadata or NetApp folder path is missing." });
        }

        if (caseMetadata.ActiveTransferId.HasValue)
        {
            _logger.LogWarning(
                "Active transfer {TransferId} in progress for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                caseMetadata.ActiveTransferId.Value, request.CaseId, correlationId);
            return new ConflictObjectResult(
                "A case-wide file transfer is in progress. Please wait for it to complete before starting a move operation.");
        }

        var casePrefix = EnsureTrailingSlash(caseMetadata.NetappFolderPath);
        var destinationPrefix = EnsureTrailingSlash(request.DestinationPrefix);

        if (!destinationPrefix.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Destination prefix '{DestinationPrefix}' is outside the case folder '{CasePrefix}'. CorrelationId: {CorrelationId}",
                destinationPrefix, casePrefix, correlationId);
            return new BadRequestObjectResult(new[] { "The destination prefix is not within the case's NetApp folder." });
        }

        var invalidPaths = request.Operations
            .Where(op => !op.SourcePath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
            .Select(op => op.SourcePath)
            .ToList();

        if (invalidPaths.Count == 0)
        {
            return null;
        }

        _logger.LogWarning("Source paths outside case folder: {Paths}. CorrelationId: {CorrelationId}",
            invalidPaths, correlationId);
        return new BadRequestObjectResult(new[] { $"The following source paths are not within the case's NetApp folder: {string.Join(", ", invalidPaths)}" });
    }

    private async Task ExecuteMovesAsync(
        IEnumerable<MoveNetAppBatchOperationDto> operations,
        string destinationPrefix,
        string bearerToken,
        Guid volumeUuid,
        List<MoveNetAppBatchItemResult> results)
    {
        foreach (var operation in operations)
        {
            results.Add(await MoveSingleOperationAsync(operation, destinationPrefix, bearerToken, volumeUuid));
        }
    }

    private async Task<MoveNetAppBatchItemResult> MoveSingleOperationAsync(
        MoveNetAppBatchOperationDto operation,
        string destinationPrefix,
        string bearerToken,
        Guid volumeUuid)
    {
        var isFolder = operation.Type == NetAppBatchOperationType.Folder;
        var destinationPath = BuildDestinationPath(operation.SourcePath, destinationPrefix, isFolder);

        var preMoveResult = ValidateOperation(operation, destinationPath, isFolder);
        if (preMoveResult is not null)
        {
            _logger.LogWarning("Skipping move for {SourcePath}: {Status} - {Error}",
                operation.SourcePath, preMoveResult.Status, preMoveResult.Error);
            return preMoveResult;
        }

        var arg = _ontapArgFactory.CreateMaterialRenameArg(bearerToken, volumeUuid, operation.SourcePath, destinationPath);

        try
        {
            var result = await _ontapHttpClient.RenameMaterialAsync(arg);
            return CreateItemResult(operation, destinationPath, MapRenameResultToStatus(result), result.Success ? null : result.ErrorMessage);
        }
        catch (Exception ex) when (IsAuthException(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error moving {SourcePath} to {DestinationPath}.", operation.SourcePath, destinationPath);
            return CreateItemResult(operation, destinationPath, OperationResultStatus.Failed, ex.Message);
        }
    }

    private async Task WriteMoveActivityLogAsync(MoveNetAppBatchDto request, List<MoveNetAppBatchItemResult> results, string userName)
    {
        var hasAuditableResult = results.Any(r => r.Status is OperationResultStatus.Moved or OperationResultStatus.NotFound);
        if (!hasAuditableResult)
        {
            return;
        }

        try
        {
            var auditedPaths = results
                .Where(r => r.Status is OperationResultStatus.Moved or OperationResultStatus.NotFound)
                .Select(r => r.SourcePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var auditedOps = request.Operations
                .Where(op => auditedPaths.Contains(op.SourcePath))
                .ToList();

            var hasFolder = auditedOps.Any(op => op.Type == NetAppBatchOperationType.Folder);
            var hasMaterial = auditedOps.Any(op => op.Type == NetAppBatchOperationType.Material);

            var (actionType, resourceType) = (hasFolder, hasMaterial) switch
            {
                (true, true) => (ActivityLog.Enums.ActionType.FolderAndMaterialMoved, ActivityLog.Enums.ResourceType.Material),
                (true, false) => (ActivityLog.Enums.ActionType.FolderMoved, ActivityLog.Enums.ResourceType.NetAppFolder),
                _ => (ActivityLog.Enums.ActionType.MaterialMoved, ActivityLog.Enums.ResourceType.Material),
            };

            var details = new
            {
                items = results.Select(r => new
                {
                    sourcePath = r.SourcePath,
                    destinationPath = r.DestinationPath,
                    outcome = r.Status,
                    type = r.Type,
                    error = r.Error,
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
            _logger.LogError(ex, "Failed to write batch move activity log for case {CaseId}.", request.CaseId);
        }
    }

    private async Task ReleaseManageMaterialsLockAsync(Guid operationId, int caseId)
    {
        try
        {
            await _caseActiveManageMaterialsService.DeleteOperationAsync(operationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release manage materials lock {OperationId} for case {CaseId}.",
                operationId, caseId);
        }
    }

    private static CaseActiveManageMaterialsOperation CreateManageMaterialsOperation(
        MoveNetAppBatchDto request,
        string destinationPrefix,
        string userName)
    {
        var sourcePaths = request.Operations.Select(op => op.SourcePath).ToList();
        var destinationPaths = new List<string> { destinationPrefix };

        return new CaseActiveManageMaterialsOperation
        {
            Id = Guid.NewGuid(),
            CaseId = request.CaseId,
            OperationType = "BatchMove",
            SourcePaths = JsonSerializer.Serialize(sourcePaths),
            DestinationPaths = JsonSerializer.Serialize(destinationPaths),
            UserName = userName,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static MoveNetAppBatchResponse BuildBatchResponse(int totalRequested, List<MoveNetAppBatchItemResult> results)
    {
        var succeeded = results.Count(r => r.Status == OperationResultStatus.Moved);
        var notFound = results.Count(r => r.Status == OperationResultStatus.NotFound);
        var alreadyInPlace = results.Count(r => r.Status == OperationResultStatus.AlreadyInPlace);
        var failed = results.Count(r => r.Status is OperationResultStatus.Failed or OperationResultStatus.Conflict);

        return new MoveNetAppBatchResponse
        {
            Status = NetAppBatchOutcome.ResolveStatus(succeeded, failed, notFound + alreadyInPlace),
            TotalRequested = totalRequested,
            Succeeded = succeeded,
            NotFound = notFound,
            AlreadyInPlace = alreadyInPlace,
            Failed = failed,
            Results = results,
        };
    }

    private static MoveNetAppBatchItemResult CreateItemResult(
        MoveNetAppBatchOperationDto operation,
        string destinationPath,
        string status,
        string? error) =>
        new()
        {
            Type = operation.Type.ToString(),
            SourcePath = operation.SourcePath,
            DestinationPath = destinationPath,
            Status = status,
            Error = error,
        };

    private static string MapRenameResultToStatus(MaterialRenameResult result) =>
        result switch
        {
            { Success: true } => OperationResultStatus.Moved,
            { WasFound: false } => OperationResultStatus.NotFound,
            { ErrorStatusCode: (int)HttpStatusCode.Conflict } => OperationResultStatus.Conflict,
            _ => OperationResultStatus.Failed,
        };

    private static bool IsAuthException(Exception ex) =>
        ex is OntapUnauthorizedException
            or OntapClientException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };

    private static string EnsureTrailingSlash(string path) =>
        path.EndsWith('/') ? path : path + "/";

    private static string BuildDestinationPath(string sourcePath, string destinationPrefix, bool isFolder)
    {
        var trimmed = sourcePath.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        var name = lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
        return isFolder ? $"{destinationPrefix}{name}/" : $"{destinationPrefix}{name}";
    }

    private static MoveNetAppBatchItemResult? ValidateOperation(
        MoveNetAppBatchOperationDto operation,
        string destinationPath,
        bool isFolder)
    {
        if (string.Equals(destinationPath, operation.SourcePath, StringComparison.OrdinalIgnoreCase))
        {
            return CreateItemResult(
                operation,
                destinationPath,
                OperationResultStatus.AlreadyInPlace,
                "The item is already in the destination location.");
        }

        // For folders, sourcePath ends with '/', so a destination that starts with it is the
        // folder itself or one of its descendants - a move into its own subtree.
        if (isFolder && destinationPath.StartsWith(operation.SourcePath, StringComparison.OrdinalIgnoreCase))
        {
            return CreateItemResult(
                operation,
                destinationPath,
                OperationResultStatus.Failed,
                "Cannot move a folder into itself or one of its subfolders.");
        }

        return null;
    }
}
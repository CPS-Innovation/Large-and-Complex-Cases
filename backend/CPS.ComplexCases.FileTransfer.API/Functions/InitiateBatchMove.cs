using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class InitiateBatchMove(
    ILogger<InitiateBatchMove> logger,
    ICaseMetadataService caseMetadataService,
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    IRequestValidator requestValidator,
    IInitializationHandler initializationHandler,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<InitiateBatchMove> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    private record OperationPreflightResult(
        List<MoveFileItem> FileItems,
        MoveBatchOriginalOperation? OriginalOperation,
        List<string> Errors404,
        List<string> Errors409,
        List<string> ListingErrors);

    [Function(nameof(InitiateBatchMove))]
    [OpenApiOperation(operationId: nameof(InitiateBatchMove), tags: ["NetApp"], Description = "Accepts a batch of NetApp move operations for a single case and starts asynchronous processing. Returns a transferId for status polling.")]
    [OpenApiParameter(name: HttpHeaderKeys.CorrelationId, In = Microsoft.OpenApi.Models.ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Correlation identifier for tracking the request.")]
    [OpenApiRequestBody(contentType: ContentType.ApplicationJson, bodyType: typeof(MoveNetAppBatchRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferResponse), Description = "Move batch accepted.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "netapp/move/batch")] HttpRequest req,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var correlationId = req.Headers.GetCorrelationId();
        _logger.LogInformation("InitiateBatchMove request received. CorrelationId: {CorrelationId}", correlationId);

        var validatedRequest = await _requestValidator.GetJsonBody<MoveNetAppBatchRequest, MoveNetAppBatchRequestValidator>(req);
        if (!validatedRequest.IsValid)
        {
            _logger.LogWarning("Invalid InitiateBatchMove request. CorrelationId: {CorrelationId}, Errors: {Errors}",
                correlationId, validatedRequest.ValidationErrors);
            return new BadRequestObjectResult(validatedRequest.ValidationErrors);
        }

        var request = validatedRequest.Value;
        _initializationHandler.Initialize(request.UserName ?? string.Empty, correlationId, request.CaseId);

        var activeTransferConflict = await CheckActiveTransferConflictAsync(request.CaseId, orchestrationClient);
        if (activeTransferConflict != null)
        {
            _logger.LogInformation(
                "Active transfer blocking batch move for CaseId: {CaseId}, TransferId: {TransferId}. CorrelationId: {CorrelationId}",
                request.CaseId, activeTransferConflict, correlationId);
            return new ConflictObjectResult("A case-wide file transfer is in progress. Please wait for it to complete before starting a move operation.");
        }

        var sourcePaths = request.Operations.Select(op => op.SourcePath).ToList();
        var destinationPaths = new List<string> { request.DestinationPrefix };

        var moveFileItems = new List<MoveFileItem>();
        var originalOperations = new List<MoveBatchOriginalOperation>();
        var preflight409Errors = new List<string>();
        var preflight404Errors = new List<string>();
        var preflightListingErrors = new List<string>();

        foreach (var op in request.Operations)
        {
            var result = string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase)
                ? await ProcessFolderOperationAsync(op, request, correlationId)
                : await ProcessFileOperationAsync(op, request, correlationId);

            moveFileItems.AddRange(result.FileItems);
            if (result.OriginalOperation != null) originalOperations.Add(result.OriginalOperation);
            preflight404Errors.AddRange(result.Errors404);
            preflight409Errors.AddRange(result.Errors409);
            preflightListingErrors.AddRange(result.ListingErrors);
        }

        if (preflightListingErrors.Count > 0)
        {
            _logger.LogWarning("Pre-flight listing errors for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Errors: {Errors}",
                request.CaseId, correlationId, preflightListingErrors);
            return new ObjectResult(preflightListingErrors) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        if (preflight404Errors.Count > 0)
        {
            _logger.LogWarning("Pre-flight 404 errors for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Errors: {Errors}",
                request.CaseId, correlationId, preflight404Errors);
            return new NotFoundObjectResult(preflight404Errors);
        }

        if (preflight409Errors.Count > 0)
        {
            _logger.LogWarning("Pre-flight conflict errors for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Errors: {Errors}",
                request.CaseId, correlationId, preflight409Errors);
            return new ConflictObjectResult(preflight409Errors);
        }

        var duplicateDestKeys = moveFileItems
            .GroupBy(f => f.DestinationPrefix + f.DestinationFileName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => $"Duplicate destination within batch: {g.Key}")
            .ToList();

        if (duplicateDestKeys.Count > 0)
        {
            _logger.LogWarning("Duplicate destination keys in batch for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Keys: {Keys}",
                request.CaseId, correlationId, duplicateDestKeys);
            return new BadRequestObjectResult(duplicateDestKeys);
        }

        if (moveFileItems.Count == 0)
            return new BadRequestObjectResult("No files found to move after expanding all operations.");

        var transferId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var mmOperation = new CaseActiveManageMaterialsOperation
        {
            Id = transferId,
            CaseId = request.CaseId,
            OperationType = "BatchMove",
            SourcePaths = JsonSerializer.Serialize(sourcePaths),
            DestinationPaths = JsonSerializer.Serialize(destinationPaths),
            UserName = request.UserName,
            CreatedAt = now,
        };

        var inserted = await _caseActiveManageMaterialsService.CheckConflictAndInsertAsync(
            mmOperation, sourcePaths, destinationPaths);

        if (!inserted)
        {
            _logger.LogWarning("Conflicting manage materials operation detected for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                request.CaseId, correlationId);
            return new ConflictObjectResult("A conflicting manage materials operation is already in progress for one or more of the specified paths.");
        }

        try
        {
            await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(MoveOrchestrator),
                new MoveBatchPayload
                {
                    TransferId = transferId,
                    CaseId = request.CaseId,
                    UserName = request.UserName,
                    CorrelationId = correlationId,
                    BearerToken = request.BearerToken,
                    BucketName = request.BucketName,
                    Files = moveFileItems,
                    OriginalOperations = originalOperations,
                    ManageMaterialsOperationId = transferId,
                },
                new StartOrchestrationOptions
                {
                    InstanceId = transferId.ToString(),
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule move orchestration for TransferId: {TransferId}, CaseId: {CaseId}. CorrelationId: {CorrelationId}. Removing active-operation row.",
                transferId, request.CaseId, correlationId);
            await _caseActiveManageMaterialsService.DeleteOperationAsync(transferId);
            throw;
        }

        _logger.LogInformation("Batch move scheduled. TransferId: {TransferId}, CaseId: {CaseId}, Files: {FileCount}. CorrelationId: {CorrelationId}",
            transferId, request.CaseId, moveFileItems.Count, correlationId);

        return new AcceptedResult($"/api/v1/filetransfer/{transferId}/status", new TransferResponse
        {
            Id = transferId,
            Status = TransferStatus.Initiated,
            CreatedAt = now,
        });
    }

    private async Task<Guid?> CheckActiveTransferConflictAsync(int caseId, DurableTaskClient orchestrationClient)
    {
        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);
        if (caseMetadata == null || !caseMetadata.ActiveTransferId.HasValue) return null;

        var entityId = new EntityInstanceId(nameof(TransferEntityState), caseMetadata.ActiveTransferId.Value.ToString());
        var entityState = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

        return entityState?.State?.Status == TransferStatus.InProgress
            ? caseMetadata.ActiveTransferId.Value
            : null;
    }

    private async Task<OperationPreflightResult> ProcessFolderOperationAsync(
        MoveNetAppBatchOperationRequest op,
        MoveNetAppBatchRequest request,
        object? correlationId)
    {
        var sourcePrefix = op.SourcePath.EndsWith('/') ? op.SourcePath : op.SourcePath + "/";
        var folderName = Path.GetFileName(op.SourcePath.TrimEnd('/'));
        var destFolderPrefix = request.DestinationPrefix + folderName + "/";

        var existsArg = _netAppArgFactory.CreateListObjectsInBucketArg(
            request.BearerToken, request.BucketName, maxKeys: 1, prefix: sourcePrefix);
        var existsResult = await _netAppClient.ListObjectsInBucketAsync(existsArg);

        if (existsResult == null || !existsResult.Data.FileData.Any())
            return EmptyResult(error404: $"Source folder not found or empty: {op.SourcePath}");

        var (expandedFiles, sourcePaginationFailed) = await ExpandSourceFilesAsync(
            request.BearerToken, request.BucketName, sourcePrefix);

        if (sourcePaginationFailed)
        {
            _logger.LogError(
                "Source folder listing returned null mid-pagination for {SourcePath}. CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                op.SourcePath, request.CaseId, correlationId);
            return EmptyResult(listingError: $"Failed to list source folder contents: {op.SourcePath}");
        }

        var (existingDestKeys, destPaginationFailed) = expandedFiles.Count > 0
            ? await GetExistingDestKeysAsync(request.BearerToken, request.BucketName, destFolderPrefix)
            : (new HashSet<string>(StringComparer.OrdinalIgnoreCase), false);

        if (destPaginationFailed)
        {
            _logger.LogError(
                "Destination folder listing returned null mid-pagination for {DestFolderPrefix}. CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                destFolderPrefix, request.CaseId, correlationId);
            return EmptyResult(listingError: $"Failed to list destination folder contents: {destFolderPrefix}");
        }

        var (fileItems, errors409, scheduledFolderKeys) = BuildFolderMoveItems(expandedFiles, destFolderPrefix, existingDestKeys);

        var originalOp = new MoveBatchOriginalOperation
        {
            Type = op.Type,
            SourcePath = op.SourcePath,
            DestinationPrefix = destFolderPrefix,
            ExpectedSourceKeys = scheduledFolderKeys,
        };

        return new OperationPreflightResult(fileItems, originalOp, [], errors409, []);
    }

    private async Task<OperationPreflightResult> ProcessFileOperationAsync(
        MoveNetAppBatchOperationRequest op,
        MoveNetAppBatchRequest request,
        object? correlationId)
    {
        var sourceArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, op.SourcePath);
        if (!await _netAppClient.DoesObjectExistAsync(sourceArg))
            return EmptyResult(error404: $"Source file not found: {op.SourcePath}");

        var fileName = Path.GetFileName(op.SourcePath);
        var computedDest = request.DestinationPrefix + fileName;

        var destArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, computedDest);
        if (await _netAppClient.DoesObjectExistAsync(destArg))
            return EmptyResult(error409: $"A file already exists at the destination: {computedDest}");

        var (caseClashFound, caseClashListingFailed) = await CheckCaseInsensitiveClashAsync(
            request.BearerToken, request.BucketName, request.DestinationPrefix, computedDest);

        if (caseClashListingFailed)
        {
            _logger.LogError(
                "Destination listing returned null mid-pagination during case-insensitive clash check for {SourcePath}. CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                op.SourcePath, request.CaseId, correlationId);
            return EmptyResult(listingError: $"Failed to list destination folder contents during clash check: {op.SourcePath}");
        }

        if (caseClashFound)
            return EmptyResult(error409: $"A case-insensitive name clash exists at the destination for: {op.SourcePath}");

        var fileItem = new MoveFileItem
        {
            SourceKey = op.SourcePath,
            DestinationPrefix = request.DestinationPrefix,
            DestinationFileName = fileName,
        };

        var originalOp = new MoveBatchOriginalOperation
        {
            Type = op.Type,
            SourcePath = op.SourcePath,
            DestinationPrefix = request.DestinationPrefix,
        };

        return new OperationPreflightResult([fileItem], originalOp, [], [], []);
    }

    private async Task<(List<(string SourceKey, string RelativeKey)> Files, bool PaginationFailed)> ExpandSourceFilesAsync(
        string bearerToken, string bucketName, string sourcePrefix)
    {
        var files = new List<(string SourceKey, string RelativeKey)>();
        string? continuationToken = null;

        do
        {
            var listArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken, bucketName,
                continuationToken: continuationToken,
                prefix: sourcePrefix);
            var listResult = await _netAppClient.ListObjectsInBucketAsync(listArg);

            if (listResult == null) return (files, true);

            foreach (var file in listResult.Data.FileData)
            {
                var relativeKey = file.Path[sourcePrefix.Length..];
                if (relativeKey.Length == 0)
                {
                    _logger.LogWarning("Empty relative key found for file: {FilePath}. SourcePrefix: {SourcePrefix}",
                        file.Path, sourcePrefix);
                    continue;
                }
                files.Add((file.Path, relativeKey));
            }

            continuationToken = listResult.Pagination.NextContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return (files, false);
    }

    private async Task<(HashSet<string> Keys, bool PaginationFailed)> GetExistingDestKeysAsync(
        string bearerToken, string bucketName, string destFolderPrefix)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? continuationToken = null;

        do
        {
            var listArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken, bucketName,
                continuationToken: continuationToken,
                prefix: destFolderPrefix);
            var result = await _netAppClient.ListObjectsInBucketAsync(listArg);

            if (result == null) return (keys, true);

            foreach (var f in result.Data.FileData)
                keys.Add(f.Path);

            continuationToken = result.Pagination.NextContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return (keys, false);
    }

    private async Task<(bool ClashFound, bool ListingFailed)> CheckCaseInsensitiveClashAsync(
        string bearerToken, string bucketName, string destinationPrefix, string computedDest)
    {
        string? continuationToken = null;

        do
        {
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken, bucketName,
                continuationToken: continuationToken,
                prefix: destinationPrefix);
            var result = await _netAppClient.ListObjectsInBucketAsync(arg);

            if (result == null) return (false, true);

            if (result.Data.FileData.Any(f =>
                    string.Equals(f.Path, computedDest, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(f.Path, computedDest, StringComparison.Ordinal)))
                return (true, false);

            continuationToken = result.Pagination.NextContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return (false, false);
    }

    private static (List<MoveFileItem> FileItems, List<string> Errors409, List<string> ScheduledKeys) BuildFolderMoveItems(
        List<(string SourceKey, string RelativeKey)> expandedFiles,
        string destFolderPrefix,
        HashSet<string> existingDestKeys)
    {
        var fileItems = new List<MoveFileItem>();
        var errors409 = new List<string>();
        var scheduledKeys = new List<string>();

        foreach (var (sourceKey, relativeKey) in expandedFiles)
        {
            var destKey = destFolderPrefix + relativeKey;
            if (existingDestKeys.Contains(destKey))
            {
                errors409.Add($"A file already exists at the destination: {destKey}");
                continue;
            }

            fileItems.Add(new MoveFileItem
            {
                SourceKey = sourceKey,
                DestinationPrefix = destFolderPrefix,
                DestinationFileName = relativeKey,
            });
            scheduledKeys.Add(sourceKey);
        }

        return (fileItems, errors409, scheduledKeys);
    }

    private static OperationPreflightResult EmptyResult(
        string? error404 = null,
        string? error409 = null,
        string? listingError = null) =>
        new(
            [],
            null,
            error404 != null ? [error404] : [],
            error409 != null ? [error409] : [],
            listingError != null ? [listingError] : []);
}

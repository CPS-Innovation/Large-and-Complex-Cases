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

public class InitiateBatchCopy(
    ILogger<InitiateBatchCopy> logger,
    ICaseMetadataService caseMetadataService,
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    IRequestValidator requestValidator,
    IInitializationHandler initializationHandler,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory)
{
    private readonly ILogger<InitiateBatchCopy> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(InitiateBatchCopy))]
    [OpenApiOperation(operationId: nameof(InitiateBatchCopy), tags: ["NetApp"], Description = "Accepts a batch of NetApp copy operations for a single case and starts asynchronous processing. Returns a transferId for status polling.")]
    [OpenApiParameter(name: HttpHeaderKeys.CorrelationId, In = Microsoft.OpenApi.Models.ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Correlation identifier for tracking the request.")]
    [OpenApiRequestBody(contentType: ContentType.ApplicationJson, bodyType: typeof(CopyNetAppBatchRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferResponse), Description = "Copy batch accepted.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "netapp/copy/batch")] HttpRequest req,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var correlationId = req.Headers.GetCorrelationId();
        _logger.LogInformation("InitiateBatchCopy request received. CorrelationId: {CorrelationId}", correlationId);

        var validatedRequest = await _requestValidator.GetJsonBody<CopyNetAppBatchRequest, CopyNetAppBatchRequestValidator>(req);
        if (!validatedRequest.IsValid)
        {
            _logger.LogWarning("Invalid InitiateBatchCopy request. CorrelationId: {CorrelationId}, Errors: {Errors}",
                correlationId, validatedRequest.ValidationErrors);
            return new BadRequestObjectResult(validatedRequest.ValidationErrors);
        }

        var request = validatedRequest.Value;
        _initializationHandler.Initialize(request.UserName ?? string.Empty, correlationId, request.CaseId);

        // Check for an active case-wide transfer (ActiveTransferId blocks all MM operations)
        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(request.CaseId);
        if (caseMetadata?.ActiveTransferId.HasValue == true)
        {
            var entityId = new EntityInstanceId(nameof(TransferEntityState), caseMetadata.ActiveTransferId.Value.ToString());
            var entityState = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

            if (entityState?.State?.Status == TransferStatus.InProgress)
            {
                _logger.LogInformation(
                    "Active transfer blocking batch copy for CaseId: {CaseId}, TransferId: {TransferId}. CorrelationId: {CorrelationId}",
                    request.CaseId, caseMetadata.ActiveTransferId.Value, correlationId);
                return new ConflictObjectResult("A case-wide file transfer is in progress. Please wait for it to complete before starting a copy operation.");
            }
        }

        var sourcePaths = request.Operations.Select(op => op.SourcePath).ToList();
        var destinationPaths = new List<string> { request.DestinationPrefix };

        // Pre-flight checks and folder expansion
        var copyFileItems = new List<CopyFileItem>();
        var originalOperations = new List<CopyBatchOriginalOperation>();
        var preflight409Errors = new List<string>();
        var preflight404Errors = new List<string>();

        foreach (var op in request.Operations)
        {
            if (string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                var sourcePrefix = op.SourcePath.EndsWith('/') ? op.SourcePath : op.SourcePath + "/";
                var folderName = Path.GetFileName(op.SourcePath.TrimEnd('/'));
                var destFolderPrefix = request.DestinationPrefix + folderName + "/";

                // Verify source folder exists (MaxKeys=1)
                var existsArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                    request.BearerToken, request.BucketName, maxKeys: 1, prefix: sourcePrefix);
                var existsResult = await _netAppClient.ListObjectsInBucketAsync(existsArg);

                if (existsResult == null || !existsResult.Data.FileData.Any())
                {
                    preflight404Errors.Add($"Source folder not found or empty: {op.SourcePath}");
                    continue;
                }

                // Expand all keys in the folder with pagination
                var expandedFiles = new List<(string SourceKey, string RelativeKey)>();
                string? continuationToken = null;
                do
                {
                    var listArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                        request.BearerToken, request.BucketName,
                        continuationToken: continuationToken,
                        prefix: sourcePrefix);

                    var listResult = await _netAppClient.ListObjectsInBucketAsync(listArg);
                    if (listResult == null) break;

                    foreach (var file in listResult.Data.FileData)
                    {
                        var relativeKey = file.Path.Substring(sourcePrefix.Length);
                        expandedFiles.Add((file.Path, relativeKey));
                    }

                    continuationToken = listResult.Pagination.NextContinuationToken;
                }
                while (!string.IsNullOrEmpty(continuationToken));

                // List the destination folder once to detect any pre-existing files that would be overwritten
                var existingDestKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string? destContinuationToken = null;
                do
                {
                    var destListArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                        request.BearerToken, request.BucketName,
                        continuationToken: destContinuationToken,
                        prefix: destFolderPrefix);
                    var destListResult = await _netAppClient.ListObjectsInBucketAsync(destListArg);
                    if (destListResult == null) break;
                    foreach (var f in destListResult.Data.FileData)
                        existingDestKeys.Add(f.Path);
                    destContinuationToken = destListResult.Pagination.NextContinuationToken;
                }
                while (!string.IsNullOrEmpty(destContinuationToken));

                // Schedule non-colliding files; track scheduled keys so WriteCopyActivityLog can
                // compare all expected keys against successes and detect partial folder copies.
                var scheduledFolderKeys = new List<string>();
                foreach (var (sourceKey, relativeKey) in expandedFiles)
                {
                    var destKey = destFolderPrefix + relativeKey;
                    if (existingDestKeys.Contains(destKey))
                    {
                        preflight409Errors.Add($"A file already exists at the destination: {destKey}");
                        continue;
                    }
                    copyFileItems.Add(new CopyFileItem
                    {
                        SourceKey = sourceKey,
                        DestinationPrefix = destFolderPrefix,
                        DestinationFileName = relativeKey,
                    });
                    scheduledFolderKeys.Add(sourceKey);
                }

                originalOperations.Add(new CopyBatchOriginalOperation
                {
                    Type = op.Type,
                    SourcePath = op.SourcePath,
                    DestinationPrefix = destFolderPrefix,
                    ExpectedSourceKeys = scheduledFolderKeys,
                });
            }
            else
            {
                // Material operation
                var sourceArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, op.SourcePath);
                var sourceExists = await _netAppClient.DoesObjectExistAsync(sourceArg);
                if (!sourceExists)
                {
                    preflight404Errors.Add($"Source file not found: {op.SourcePath}");
                    continue;
                }

                var fileName = Path.GetFileName(op.SourcePath);
                var computedDest = request.DestinationPrefix + fileName;

                var destArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, computedDest);
                var destExists = await _netAppClient.DoesObjectExistAsync(destArg);
                if (destExists)
                {
                    preflight409Errors.Add($"A file already exists at the destination: {computedDest}");
                    continue;
                }

                // Case-insensitive clash check: list destination prefix and look for same name case-insensitively
                var caseCheckArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                    request.BearerToken, request.BucketName,
                    prefix: request.DestinationPrefix);
                var caseCheckResult = await _netAppClient.ListObjectsInBucketAsync(caseCheckArg);
                if (caseCheckResult != null)
                {
                    var clash = caseCheckResult.Data.FileData
                        .Any(f => string.Equals(
                            Path.GetFileName(f.Path), fileName, StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(f.Path, computedDest, StringComparison.Ordinal));

                    if (clash)
                    {
                        preflight409Errors.Add($"A case-insensitive name clash exists at the destination for: {op.SourcePath}");
                        continue;
                    }
                }

                copyFileItems.Add(new CopyFileItem
                {
                    SourceKey = op.SourcePath,
                    DestinationPrefix = request.DestinationPrefix,
                    DestinationFileName = fileName,
                });

                originalOperations.Add(new CopyBatchOriginalOperation
                {
                    Type = op.Type,
                    SourcePath = op.SourcePath,
                    DestinationPrefix = request.DestinationPrefix,
                });
            }
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

        if (copyFileItems.Count == 0)
        {
            return new BadRequestObjectResult("No files found to copy after expanding all operations.");
        }

        var transferId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Atomically check for a conflicting operation and insert the lock row in one serializable
        // transaction, so two concurrent requests cannot both observe no conflict and both proceed.
        var mmOperation = new CaseActiveManageMaterialsOperation
        {
            Id = transferId,
            CaseId = request.CaseId,
            OperationType = "BatchCopy",
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
                nameof(CopyOrchestrator),
                new CopyBatchPayload
                {
                    TransferId = transferId,
                    CaseId = request.CaseId,
                    UserName = request.UserName,
                    CorrelationId = correlationId,
                    BearerToken = request.BearerToken,
                    BucketName = request.BucketName,
                    Files = copyFileItems,
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
            _logger.LogError(ex, "Failed to schedule copy orchestration for TransferId: {TransferId}, CaseId: {CaseId}. CorrelationId: {CorrelationId}. Removing active-operation row.",
                transferId, request.CaseId, correlationId);
            await _caseActiveManageMaterialsService.DeleteOperationAsync(transferId);
            throw;
        }

        _logger.LogInformation("Batch copy scheduled. TransferId: {TransferId}, CaseId: {CaseId}, Files: {FileCount}. CorrelationId: {CorrelationId}",
            transferId, request.CaseId, copyFileItems.Count, correlationId);

        return new AcceptedResult($"/api/v1/filetransfer/{transferId}/status", new TransferResponse
        {
            Id = transferId,
            Status = TransferStatus.Initiated,
            CreatedAt = now,
        });
    }
}

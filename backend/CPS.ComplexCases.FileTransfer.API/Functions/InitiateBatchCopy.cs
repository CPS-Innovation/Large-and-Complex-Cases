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

        var activeTransferConflict = await TryGetBlockingActiveTransferConflictAsync(
            request.CaseId, correlationId, orchestrationClient);
        if (activeTransferConflict is not null)
        {
            return activeTransferConflict;
        }

        var preflight = await BuildPreflightCopyPlanAsync(request);
        var preflightResult = ToPreflightActionResult(
            preflight, request.CaseId, correlationId);
        if (preflightResult is not null)
        {
            return preflightResult;
        }

        var sourcePaths = request.Operations.Select(op => op.SourcePath).ToList();
        var destinationPaths = new List<string> { request.DestinationPrefix };
        var transferId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var lockConflict = await TryAcquireManageMaterialsLockAsync(
            transferId, request, sourcePaths, destinationPaths, now, correlationId);
        if (lockConflict is not null)
        {
            return lockConflict;
        }

        await ScheduleCopyOrchestrationOrRollbackAsync(
            orchestrationClient,
            transferId,
            request,
            correlationId,
            preflight.CopyFileItems,
            preflight.OriginalOperations);

        _logger.LogInformation("Batch copy scheduled. TransferId: {TransferId}, CaseId: {CaseId}, Files: {FileCount}. CorrelationId: {CorrelationId}",
            transferId, request.CaseId, preflight.CopyFileItems.Count, correlationId);

        return new AcceptedResult($"/api/v1/filetransfer/{transferId}/status", new TransferResponse
        {
            Id = transferId,
            Status = TransferStatus.Initiated,
            CreatedAt = now,
        });
    }

    private async Task<IActionResult?> TryGetBlockingActiveTransferConflictAsync(
        int caseId,
        Guid? correlationId,
        DurableTaskClient orchestrationClient)
    {
        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);
        var activeTransferId = caseMetadata?.ActiveTransferId;
        if (activeTransferId is null)
        {
            return null;
        }

        var entityId = new EntityInstanceId(nameof(TransferEntityState), activeTransferId.Value.ToString());
        var entityState = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

        if (entityState?.State?.Status != TransferStatus.InProgress)
        {
            return null;
        }

        _logger.LogInformation(
            "Active transfer blocking batch copy for CaseId: {CaseId}, TransferId: {TransferId}. CorrelationId: {CorrelationId}",
            caseId, activeTransferId.Value, correlationId);
        return new ConflictObjectResult(
            "A case-wide file transfer is in progress. Please wait for it to complete before starting a copy operation.");
    }

    private async Task<PreflightCopyPlan> BuildPreflightCopyPlanAsync(CopyNetAppBatchRequest request)
    {
        var plan = new PreflightCopyPlan();

        foreach (var op in request.Operations)
        {
            if (string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                await ExpandFolderOperationAsync(request, op, plan);
            }
            else
            {
                await ValidateAndPlanMaterialCopyAsync(request, op, plan);
            }
        }

        return plan;
    }

    private async Task ExpandFolderOperationAsync(
        CopyNetAppBatchRequest request,
        CopyNetAppBatchOperationRequest op,
        PreflightCopyPlan plan)
    {
        var sourcePrefix = op.SourcePath.EndsWith('/') ? op.SourcePath : op.SourcePath + "/";
        var folderName = Path.GetFileName(op.SourcePath.TrimEnd('/'));
        var destFolderPrefix = request.DestinationPrefix + folderName + "/";

        var existsArg = _netAppArgFactory.CreateListObjectsInBucketArg(
            request.BearerToken, request.BucketName, maxKeys: 1, prefix: sourcePrefix);
        var existsResult = await _netAppClient.ListObjectsInBucketAsync(existsArg);

        if (existsResult == null || !existsResult.Data.FileData.Any())
        {
            plan.Errors404.Add($"Source folder not found or empty: {op.SourcePath}");
            return;
        }

        var expandedFiles = await ListAllObjectKeysUnderPrefixAsync(
            request.BearerToken, request.BucketName, sourcePrefix);
        var existingDestKeys = new HashSet<string>(
            await ListAllObjectKeysUnderPrefixAsync(
                request.BearerToken, request.BucketName, destFolderPrefix),
            StringComparer.OrdinalIgnoreCase);

        var scheduledFolderKeys = new List<string>();
        foreach (var sourceKey in expandedFiles)
        {
            var relativeKey = sourceKey.Substring(sourcePrefix.Length);
            var destKey = destFolderPrefix + relativeKey;
            if (existingDestKeys.Contains(destKey))
            {
                plan.Errors409.Add($"A file already exists at the destination: {destKey}");
                continue;
            }

            plan.CopyFileItems.Add(new CopyFileItem
            {
                SourceKey = sourceKey,
                DestinationPrefix = destFolderPrefix,
                DestinationFileName = relativeKey,
            });
            scheduledFolderKeys.Add(sourceKey);
        }

        plan.OriginalOperations.Add(new CopyBatchOriginalOperation
        {
            Type = op.Type,
            SourcePath = op.SourcePath,
            DestinationPrefix = destFolderPrefix,
            ExpectedSourceKeys = scheduledFolderKeys,
        });
    }

    private async Task<List<string>> ListAllObjectKeysUnderPrefixAsync(
        string bearerToken,
        string bucketName,
        string prefix)
    {
        var keys = new List<string>();
        string? continuationToken = null;
        do
        {
            var listArg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken, bucketName,
                continuationToken: continuationToken,
                prefix: prefix);

            var listResult = await _netAppClient.ListObjectsInBucketAsync(listArg);
            if (listResult == null) break;

            foreach (var file in listResult.Data.FileData)
            {
                keys.Add(file.Path);
            }

            continuationToken = listResult.Pagination.NextContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return keys;
    }

    private async Task ValidateAndPlanMaterialCopyAsync(
        CopyNetAppBatchRequest request,
        CopyNetAppBatchOperationRequest op,
        PreflightCopyPlan plan)
    {
        var sourceArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, op.SourcePath);
        var sourceExists = await _netAppClient.DoesObjectExistAsync(sourceArg);
        if (!sourceExists)
        {
            plan.Errors404.Add($"Source file not found: {op.SourcePath}");
            return;
        }

        var fileName = Path.GetFileName(op.SourcePath);
        var computedDest = request.DestinationPrefix + fileName;

        var destArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, computedDest);
        var destExists = await _netAppClient.DoesObjectExistAsync(destArg);
        if (destExists)
        {
            plan.Errors409.Add($"A file already exists at the destination: {computedDest}");
            return;
        }

        if (await HasCaseInsensitiveDestinationClashAsync(
                request.BearerToken, request.BucketName, request.DestinationPrefix, fileName, computedDest))
        {
            plan.Errors409.Add($"A case-insensitive name clash exists at the destination for: {op.SourcePath}");
            return;
        }

        plan.CopyFileItems.Add(new CopyFileItem
        {
            SourceKey = op.SourcePath,
            DestinationPrefix = request.DestinationPrefix,
            DestinationFileName = fileName,
        });

        plan.OriginalOperations.Add(new CopyBatchOriginalOperation
        {
            Type = op.Type,
            SourcePath = op.SourcePath,
            DestinationPrefix = request.DestinationPrefix,
        });
    }

    private async Task<bool> HasCaseInsensitiveDestinationClashAsync(
        string bearerToken,
        string bucketName,
        string destinationPrefix,
        string fileName,
        string computedDest)
    {
        var caseCheckArg = _netAppArgFactory.CreateListObjectsInBucketArg(
            bearerToken, bucketName, prefix: destinationPrefix);
        var caseCheckResult = await _netAppClient.ListObjectsInBucketAsync(caseCheckArg);
        if (caseCheckResult == null)
        {
            return false;
        }

        return caseCheckResult.Data.FileData.Any(f =>
            string.Equals(Path.GetFileName(f.Path), fileName, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(f.Path, computedDest, StringComparison.Ordinal));
    }

    internal static IActionResult? ToPreflightActionResult(
        PreflightCopyPlan plan,
        int caseId,
        Guid? correlationId,
        ILogger? logger = null)
    {
        if (plan.Errors404.Count > 0)
        {
            logger?.LogWarning("Pre-flight 404 errors for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Errors: {Errors}",
                caseId, correlationId, plan.Errors404);
            return new NotFoundObjectResult(plan.Errors404);
        }

        if (plan.Errors409.Count > 0)
        {
            logger?.LogWarning("Pre-flight conflict errors for CaseId: {CaseId}. CorrelationId: {CorrelationId}. Errors: {Errors}",
                caseId, correlationId, plan.Errors409);
            return new ConflictObjectResult(plan.Errors409);
        }

        if (plan.CopyFileItems.Count == 0)
        {
            return new BadRequestObjectResult("No files found to copy after expanding all operations.");
        }

        return null;
    }

    private IActionResult? ToPreflightActionResult(PreflightCopyPlan plan, int caseId, Guid? correlationId) =>
        ToPreflightActionResult(plan, caseId, correlationId, _logger);

    private async Task<IActionResult?> TryAcquireManageMaterialsLockAsync(
        Guid transferId,
        CopyNetAppBatchRequest request,
        List<string> sourcePaths,
        List<string> destinationPaths,
        DateTime now,
        Guid? correlationId)
    {
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

        if (inserted)
        {
            return null;
        }

        _logger.LogWarning("Conflicting manage materials operation detected for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
            request.CaseId, correlationId);
        return new ConflictObjectResult(
            "A conflicting manage materials operation is already in progress for one or more of the specified paths.");
    }

    private async Task ScheduleCopyOrchestrationOrRollbackAsync(
        DurableTaskClient orchestrationClient,
        Guid transferId,
        CopyNetAppBatchRequest request,
        Guid? correlationId,
        List<CopyFileItem> copyFileItems,
        List<CopyBatchOriginalOperation> originalOperations)
    {
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
    }

    internal sealed class PreflightCopyPlan
    {
        public List<CopyFileItem> CopyFileItems { get; } = [];
        public List<CopyBatchOriginalOperation> OriginalOperations { get; } = [];
        public List<string> Errors404 { get; } = [];
        public List<string> Errors409 { get; } = [];
    }
}

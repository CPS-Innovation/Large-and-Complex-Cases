using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public abstract class BatchOrchestratorBase<TPayload, TFileItem>(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
    where TPayload : IBatchPayload<TFileItem>
    where TFileItem : IBatchFileItem
{
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;
    protected readonly ITelemetryClient _telemetryClient = telemetryClient;
    protected readonly IInitializationHandler _initializationHandler = initializationHandler;

    protected abstract TransferType BatchTransferType { get; }

    /// <summary>
    /// Runs between the retry phase and FinalizeTransfer. Returns the list of
    /// successfully transferred source keys to pass to the activity log.
    /// For Copy this is a simple filter over allResults; for Move it also deletes
    /// source files and empty folders.
    /// </summary>
    protected abstract Task<List<string>> GetSuccessfulSourceKeysAsync(
        TaskOrchestrationContext context,
        TPayload input,
        List<TransferResult> allResults);

    protected abstract Task WriteActivityLogAsync(
        TaskOrchestrationContext context,
        TPayload input,
        List<string> successfulSourceKeys);

    protected async Task RunOrchestratorAsync(TaskOrchestrationContext context, string orchestratorName)
    {
        ILogger logger = context.CreateReplaySafeLogger(orchestratorName);
        logger.LogInformation("{OrchestratorName} started.", orchestratorName);

        var input = context.GetInput<TPayload>();
        if (input == null)
        {
            logger.LogError("{OrchestratorName} input is null.", orchestratorName);
            throw new ArgumentNullException(nameof(input));
        }

        _initializationHandler.Initialize(input.UserName!, input.CorrelationId);

        var telemetryEvent = new TransferOrchestrationEvent
        {
            TransferDirection = TransferDirection.NetAppToNetApp.ToString(),
            TotalFiles = input.Files.Count,
            BucketName = input.BucketName,
            CaseId = input.CaseId,
            OrchestrationStartTime = DateTime.UtcNow
        };

        var completedSuccessfully = false;
        try
        {
            var entityId = await InitializeTransferAsync(context, input);

            TransferFilePayload BuildPayload(TFileItem f) => BuildTransferFilePayload(
                f, input.CaseId, input.TransferId, input.BearerToken,
                input.BucketName, input.UserName!, input.CorrelationId, BatchTransferType);

            var allResults = await FanOutTransferFilesAsync(context, input.Files, BuildPayload, entityId, telemetryEvent);

            await RetryTransientFailuresAsync(context, entityId, input.Files, allResults, BuildPayload, telemetryEvent, logger, orchestratorName);

            var successfulSourceKeys = await GetSuccessfulSourceKeysAsync(context, input, allResults);

            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload { TransferId = input.TransferId });

            await WriteActivityLogAsync(context, input, successfulSourceKeys);

            telemetryEvent.IsSuccessful = telemetryEvent.TotalFilesFailed == 0;
            completedSuccessfully = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{OrchestratorName} failed for TransferId: {TransferId}. CorrelationId: {CorrelationId}",
                orchestratorName, input.TransferId, input.CorrelationId);

            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.Failed,
                });

            throw;
        }
        finally
        {
            await context.CallActivityAsync(
                nameof(RemoveActiveManageMaterialsOperation),
                input.ManageMaterialsOperationId);

            telemetryEvent.IsSuccessful = completedSuccessfully && telemetryEvent.TotalFilesFailed == 0;
            telemetryEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
    }

    private async Task<EntityInstanceId> InitializeTransferAsync(TaskOrchestrationContext context, TPayload input)
    {
        var transferEntity = new TransferEntity
        {
            Id = input.TransferId,
            Status = TransferStatus.Initiated,
            DestinationPath = string.Empty,
            SourcePaths = input.Files.Select(f => new TransferSourcePath { Path = f.SourceKey }).ToList(),
            CaseId = input.CaseId,
            TransferType = BatchTransferType,
            Direction = TransferDirection.NetAppToNetApp,
            TotalFiles = input.Files.Count,
            IsRetry = false,
            UserName = input.UserName,
            CorrelationId = input.CorrelationId,
            BearerToken = input.BearerToken
        };

        var entityId = new EntityInstanceId(nameof(TransferEntityState), input.TransferId.ToString());

        await context.Entities.CallEntityAsync(entityId, nameof(TransferEntityState.Initialize), transferEntity);

        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.InProgress,
            });

        return entityId;
    }

    private async Task<List<TransferResult>> FanOutTransferFilesAsync(
        TaskOrchestrationContext context,
        List<TFileItem> files,
        Func<TFileItem, TransferFilePayload> buildPayload,
        EntityInstanceId entityId,
        TransferOrchestrationEvent telemetryEvent)
    {
        int batchSize = _sizeConfig.BatchSize;
        var batch = new List<Task<TransferResult>>();
        var allResults = new List<TransferResult>();

        foreach (var fileItem in files)
        {
            batch.Add(context.CallActivityAsync<TransferResult>(nameof(TransferFile), buildPayload(fileItem)));

            if (batch.Count >= batchSize)
            {
                var batchResults = await Task.WhenAll(batch);
                await ProcessTransferResults(context, entityId, batchResults, telemetryEvent);
                allResults.AddRange(batchResults);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            var remainingResults = await Task.WhenAll(batch);
            await ProcessTransferResults(context, entityId, remainingResults, telemetryEvent);
            allResults.AddRange(remainingResults);
        }

        return allResults;
    }

    private async Task RetryTransientFailuresAsync(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        List<TFileItem> files,
        List<TransferResult> allResults,
        Func<TFileItem, TransferFilePayload> buildPayload,
        TransferOrchestrationEvent telemetryEvent,
        ILogger logger,
        string orchestratorName)
    {
        int maxOrchestratorRetries = _sizeConfig.MaxOrchestratorRetries;
        for (int attempt = 0; attempt < maxOrchestratorRetries; attempt++)
        {
            var retryableFailures = allResults
                .Where(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient)
                .ToList();

            if (retryableFailures.Count == 0) break;

            logger.LogWarning(
                "{OrchestratorName} retry attempt {Attempt}/{MaxRetries}: re-attempting {Count} transiently failed files.",
                orchestratorName, attempt + 1, maxOrchestratorRetries, retryableFailures.Count);

            await context.CreateTimer(
                context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(30 * (attempt + 1))),
                CancellationToken.None);

            var failedKeys = retryableFailures.Select(r => r.FailedItem!.SourcePath).ToHashSet();
            var retryFileItems = files.Where(f => failedKeys.Contains(f.SourceKey)).ToList();

            await context.Entities.CallEntityAsync(entityId, nameof(TransferEntityState.RemoveTransientFailures));

            telemetryEvent.TotalFilesFailed -= retryableFailures.Count;

            var retryBatch = retryFileItems
                .Select(fileItem => context.CallActivityAsync<TransferResult>(nameof(TransferFile), buildPayload(fileItem)))
                .ToList();

            var retryResults = await Task.WhenAll(retryBatch);
            await ProcessTransferResults(context, entityId, retryResults, telemetryEvent, isRetry: true);

            allResults.RemoveAll(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient);
            allResults.AddRange(retryResults);
        }
    }

    protected static async Task ProcessTransferResults(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        TransferResult[] results,
        TransferOrchestrationEvent telemetryEvent,
        bool isRetry = false)
    {
        foreach (var result in results)
        {
            if (result != null && result.IsSuccess && result.SuccessfulItem != null)
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddSuccessfulRetryItem)
                            : nameof(TransferEntityState.AddSuccessfulItem),
                    result.SuccessfulItem);

                telemetryEvent.TotalFilesTransferred++;
                telemetryEvent.TotalBytesTransferred += result.SuccessfulItem.Size;
            }
            else if (result != null && !result.IsSuccess && result.FailedItem != null)
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddFailedRetryItem)
                            : nameof(TransferEntityState.AddFailedItem),
                    result.FailedItem);

                telemetryEvent.TotalFilesFailed++;
            }
        }
    }

    protected static TransferFilePayload BuildTransferFilePayload(
        IBatchFileItem fileItem,
        int caseId,
        Guid transferId,
        string bearerToken,
        string bucketName,
        string userName,
        Guid? correlationId,
        TransferType transferType) =>
        new()
        {
            CaseId = caseId,
            SourcePath = new TransferSourcePath
            {
                Path = fileItem.SourceKey,
                ModifiedPath = fileItem.DestinationFileName,
            },
            DestinationPath = fileItem.DestinationPrefix,
            TransferId = transferId,
            TransferType = transferType,
            TransferDirection = TransferDirection.NetAppToNetApp,
            WorkspaceId = string.Empty,
            BearerToken = bearerToken,
            BucketName = bucketName,
            UserName = userName,
            CorrelationId = correlationId,
        };
}

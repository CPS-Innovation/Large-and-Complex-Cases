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

public abstract class BatchOrchestratorBase(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
{
    protected readonly SizeConfig _sizeConfig = sizeConfig.Value;
    protected readonly ITelemetryClient _telemetryClient = telemetryClient;
    protected readonly IInitializationHandler _initializationHandler = initializationHandler;

    protected static async Task<EntityInstanceId> InitializeTransferAsync<TFileItem>(
        TaskOrchestrationContext context,
        Guid transferId,
        int caseId,
        string? userName,
        Guid? correlationId,
        string bearerToken,
        IReadOnlyList<TFileItem> files,
        TransferType transferType)
        where TFileItem : IBatchFileItem
    {
        var transferEntity = new TransferEntity
        {
            Id = transferId,
            Status = TransferStatus.Initiated,
            DestinationPath = string.Empty,
            SourcePaths = files.Select(f => new TransferSourcePath { Path = f.SourceKey }).ToList(),
            CaseId = caseId,
            TransferType = transferType,
            Direction = TransferDirection.NetAppToNetApp,
            TotalFiles = files.Count,
            IsRetry = false,
            UserName = userName,
            CorrelationId = correlationId,
            BearerToken = bearerToken
        };

        var entityId = new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());

        await context.Entities.CallEntityAsync(entityId, nameof(TransferEntityState.Initialize), transferEntity);

        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = transferId,
                Status = TransferStatus.InProgress,
            });

        return entityId;
    }

    protected async Task<List<TransferResult>> FanOutTransferFilesAsync<TFileItem>(
        TaskOrchestrationContext context,
        IReadOnlyList<TFileItem> files,
        Func<TFileItem, TransferFilePayload> buildPayload,
        EntityInstanceId entityId,
        TransferOrchestrationEvent telemetryEvent)
        where TFileItem : IBatchFileItem
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

    protected async Task RetryTransientFailuresAsync<TFileItem>(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        IReadOnlyList<TFileItem> files,
        List<TransferResult> allResults,
        Func<TFileItem, TransferFilePayload> buildPayload,
        TransferOrchestrationEvent telemetryEvent,
        ILogger logger,
        string orchestratorName)
        where TFileItem : IBatchFileItem
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

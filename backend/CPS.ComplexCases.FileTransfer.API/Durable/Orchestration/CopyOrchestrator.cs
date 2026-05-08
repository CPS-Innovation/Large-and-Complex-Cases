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
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class CopyOrchestrator(IOptions<SizeConfig> sizeConfig, ITelemetryClient telemetryClient, IInitializationHandler initializationHandler)
{
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(CopyOrchestrator))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(CopyOrchestrator));
        logger.LogInformation("CopyOrchestrator started.");

        var input = context.GetInput<CopyBatchPayload>();
        if (input == null)
        {
            logger.LogError("CopyOrchestrator input is null.");
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
            // 1. Initialize transfer entity
            var transferEntity = new TransferEntity
            {
                Id = input.TransferId,
                Status = TransferStatus.Initiated,
                DestinationPath = string.Empty,
                SourcePaths = input.Files.Select(f => new TransferSourcePath { Path = f.SourceKey }).ToList(),
                CaseId = input.CaseId,
                TransferType = TransferType.Copy,
                Direction = TransferDirection.NetAppToNetApp,
                TotalFiles = input.Files.Count,
                IsRetry = false,
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                BearerToken = input.BearerToken
            };

            var entityId = new EntityInstanceId(nameof(TransferEntityState), input.TransferId.ToString());

            await context.Entities.CallEntityAsync(
                entityId,
                nameof(TransferEntityState.Initialize),
                transferEntity);

            // 2. Update status to InProgress
            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.InProgress,
                });

            // 3. Fan-out TransferFile per CopyFileItem in batches
            int batchSize = _sizeConfig.BatchSize;
            var batch = new List<Task<TransferResult>>();
            var allResults = new List<TransferResult>();

            foreach (var fileItem in input.Files)
            {
                var transferFilePayload = new TransferFilePayload
                {
                    CaseId = input.CaseId,
                    SourcePath = new TransferSourcePath
                    {
                        Path = fileItem.SourceKey,
                        ModifiedPath = fileItem.DestinationFileName,
                    },
                    DestinationPath = fileItem.DestinationPrefix,
                    TransferId = input.TransferId,
                    TransferType = TransferType.Copy,
                    TransferDirection = TransferDirection.NetAppToNetApp,
                    WorkspaceId = string.Empty,
                    BearerToken = input.BearerToken,
                    BucketName = input.BucketName,
                    UserName = input.UserName!,
                    CorrelationId = input.CorrelationId!,
                };

                batch.Add(context.CallActivityAsync<TransferResult>(nameof(TransferFile), transferFilePayload));

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

            // 4. Orchestrator-level retry for transient S3 failures
            int maxOrchestratorRetries = _sizeConfig.MaxOrchestratorRetries;
            for (int attempt = 0; attempt < maxOrchestratorRetries; attempt++)
            {
                var retryableFailures = allResults
                    .Where(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient)
                    .ToList();

                if (retryableFailures.Count == 0) break;

                logger.LogWarning(
                    "CopyOrchestrator retry attempt {Attempt}/{MaxRetries}: re-attempting {Count} transiently failed files.",
                    attempt + 1, maxOrchestratorRetries, retryableFailures.Count);

                await context.CreateTimer(
                    context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(30 * (attempt + 1))),
                    CancellationToken.None);

                var failedKeys = retryableFailures
                    .Select(r => r.FailedItem!.SourcePath)
                    .ToHashSet();

                var retryFileItems = input.Files
                    .Where(f => failedKeys.Contains(f.SourceKey))
                    .ToList();

                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.RemoveTransientFailures));

                telemetryEvent.TotalFilesFailed -= retryableFailures.Count;

                var retryBatch = retryFileItems.Select(fileItem =>
                    context.CallActivityAsync<TransferResult>(
                        nameof(TransferFile),
                        new TransferFilePayload
                        {
                            CaseId = input.CaseId,
                            SourcePath = new TransferSourcePath
                            {
                                Path = fileItem.SourceKey,
                                ModifiedPath = fileItem.DestinationFileName,
                            },
                            DestinationPath = fileItem.DestinationPrefix,
                            TransferId = input.TransferId,
                            TransferType = TransferType.Copy,
                            TransferDirection = TransferDirection.NetAppToNetApp,
                            WorkspaceId = string.Empty,
                            BearerToken = input.BearerToken,
                            BucketName = input.BucketName,
                            UserName = input.UserName!,
                            CorrelationId = input.CorrelationId!,
                        })).ToList();

                var retryResults = await Task.WhenAll(retryBatch);
                await ProcessTransferResults(context, entityId, retryResults, telemetryEvent, isRetry: true);

                allResults.RemoveAll(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient);
                allResults.AddRange(retryResults);
            }

            // 5. Finalize transfer (no delete phase — source objects are never deleted)
            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload
                {
                    TransferId = input.TransferId,
                });

            // 6. Write activity log entries per original operation
            var successfulSourceKeys = allResults
                .Where(r => r.IsSuccess && r.SuccessfulItem != null)
                .Select(r => r.SuccessfulItem!.SourcePath)
                .ToList();

            await context.CallActivityAsync(
                nameof(WriteCopyActivityLog),
                new WriteCopyActivityLogPayload
                {
                    CaseId = input.CaseId,
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId,
                    OriginalOperations = input.OriginalOperations,
                    SuccessfulSourceKeys = successfulSourceKeys,
                });

            telemetryEvent.IsSuccessful = telemetryEvent.TotalFilesFailed == 0;
            completedSuccessfully = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CopyOrchestrator failed for TransferId: {TransferId}. CorrelationId: {CorrelationId}",
                input.TransferId, input.CorrelationId);

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
            // 7. Remove case_active_manage_materials row — best effort; swallowed in the activity
            await context.CallActivityAsync(
                nameof(RemoveActiveManageMaterialsOperation),
                input.ManageMaterialsOperationId);

            telemetryEvent.IsSuccessful = completedSuccessfully && telemetryEvent.TotalFilesFailed == 0;
            telemetryEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
    }

    private static async Task ProcessTransferResults(
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
}

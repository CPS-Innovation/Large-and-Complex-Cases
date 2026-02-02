using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
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

public class TransferOrchestrator(IOptions<SizeConfig> sizeConfig, ITelemetryClient telemetryClient, IInitializationHandler initializationHandler)
{
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(TransferOrchestrator))]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(TransferOrchestrator));
        logger.LogInformation("TransferOrchestrator started.");

        var input = context.GetInput<TransferPayload>();
        if (input == null)
        {
            logger.LogError("TransferOrchestrator input is null.");
            throw new ArgumentNullException(nameof(input));
        }

        _initializationHandler.Initialize(input.UserName!, input.CorrelationId);

        var transferOrchestrationEvent = new TransferOrchestrationEvent
        {
            TransferDirection = input.TransferDirection.ToString(),
            TotalFiles = input.SourcePaths.Count,
            BucketName = input.BucketName,
            CaseId = input.CaseId,
            OrchestrationStartTime = DateTime.UtcNow
        };

        try
        {
            // 1. Initialize transfer entity
            var transferEntity = new TransferEntity
            {
                Id = input.TransferId,
                Status = TransferStatus.Initiated,
                DestinationPath = input.DestinationPath,
                SourcePaths = input.SourcePaths,
                CaseId = input.CaseId,
                TransferType = input.TransferType,
                Direction = input.TransferDirection,
                TotalFiles = input.SourcePaths.Count,
                IsRetry = input.IsRetry ?? false,
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                BearerToken = input.BearerToken
            };

            var entityId = new EntityInstanceId(nameof(TransferEntityState), input.TransferId.ToString());

            await context.Entities.CallEntityAsync(
                entityId,
                nameof(TransferEntityState.Initialize),
                transferEntity);

            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferInitiated,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId
                });

            // 2. Fan-out: TransferFileActivity for each item, throttled in batches
            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.InProgress,
                });

            int batchSize = _sizeConfig.BatchSize;
            var batch = new List<Task<TransferResult>>();

            foreach (var sourcePath in input.SourcePaths)
            {
                var transferFilePayload = new TransferFilePayload
                {
                    CaseId = input.CaseId,
                    SourcePath = sourcePath,
                    DestinationPath = transferEntity.DestinationPath,
                    TransferId = transferEntity.Id,
                    TransferType = transferEntity.TransferType,
                    TransferDirection = transferEntity.Direction,
                    WorkspaceId = input.WorkspaceId,
                    SourceRootFolderPath = input.SourceRootFolderPath,
                    BearerToken = input.BearerToken,
                    BucketName = input.BucketName,
                    UserName = input.UserName!,
                    CorrelationId = input.CorrelationId!
                };

                // Activity now returns TransferResult instead of void
                batch.Add(context.CallActivityAsync<TransferResult>(
                    nameof(TransferFile),
                    transferFilePayload));

                if (batch.Count >= batchSize)
                {
                    // Process batch results and update entity synchronously
                    var batchResults = await Task.WhenAll(batch);
                    await ProcessTransferResults(context, entityId, batchResults, transferOrchestrationEvent);
                    batch.Clear();
                }
            }

            // Process any remaining tasks in the last batch
            if (batch.Count > 0)
            {
                var remainingResults = await Task.WhenAll(batch);
                await ProcessTransferResults(context, entityId, remainingResults, transferOrchestrationEvent);
            }

            // 3. Delete files if transfer direction is EgressToNetApp and transfer type is Move
            if (input.TransferDirection == TransferDirection.EgressToNetApp && input.TransferType == TransferType.Move)
            {
                await context.CallActivityAsync(
                    nameof(DeleteFiles),
                    new DeleteFilesPayload
                    {
                        TransferId = input.TransferId,
                        TransferDirection = input.TransferDirection,
                        WorkspaceId = input.WorkspaceId,
                        UserName = input.UserName!,
                        CorrelationId = input.CorrelationId,
                        CaseId = input.CaseId
                    });
            }

            // 4. Update activity log
            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferCompleted,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId
                });

            // 5. Finalize
            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload
                {
                    TransferId = input.TransferId,
                });

            transferOrchestrationEvent.IsSuccessful = transferOrchestrationEvent.TotalFilesFailed == 0;
            transferOrchestrationEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TransferOrchestrator failed for TransferId: {TransferId}. With CorrelationId {CorrelationId}", input.TransferId, input.CorrelationId);

            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.Failed,
                });

            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferFailed,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId,
                    ExceptionMessage = ex.Message
                });

            throw;
        }
        finally
        {
            transferOrchestrationEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
    }

    /// <summary>
    /// Processes transfer results and updates the entity synchronously
    /// This eliminates the race condition by ensuring all entity updates are completed
    /// before the orchestrator continues
    /// </summary>
    private static async Task ProcessTransferResults(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        TransferResult[] results,
        TransferOrchestrationEvent telemetryEvent)
    {
        foreach (var result in results)
        {
            if (result != null && result.IsSuccess && result.SuccessfulItem != null)
            {
                // Use CallEntityAsync for synchronous entity updates
                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.AddSuccessfulItem),
                    result.SuccessfulItem);

                // Update telemetry event
                telemetryEvent.TotalFilesTransferred++;
                telemetryEvent.TotalBytesTransferred += result.SuccessfulItem.Size;
            }
            else if (result != null && !result.IsSuccess && result.FailedItem != null)
            {
                // Use CallEntityAsync for synchronous entity updates
                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.AddFailedItem),
                    result.FailedItem);

                // Update telemetry event
                telemetryEvent.TotalFilesFailed++;
            }
        }
    }
}
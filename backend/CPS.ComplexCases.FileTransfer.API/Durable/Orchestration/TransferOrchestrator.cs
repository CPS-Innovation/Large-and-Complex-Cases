using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class TransferOrchestrator(IActivityLogService activityLogService, IOptions<SizeConfig> sizeConfig)
{
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;

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

        try
        {
            // 1.  initalize transfer entity
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
                CorrelationId = input.CorrelationId,
            };

            await context.CallActivityAsync(
                nameof(InitializeTransfer),
                transferEntity);

            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferInitiated,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
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
            var batch = new List<Task>();

            foreach (var sourcePath in input.SourcePaths)
            {
                var transferFilePayload = new TransferFilePayload
                {
                    SourcePath = sourcePath,
                    DestinationPath = transferEntity.DestinationPath,
                    TransferId = transferEntity.Id,
                    TransferType = transferEntity.TransferType,
                    TransferDirection = transferEntity.Direction,
                    WorkspaceId = input.WorkspaceId,
                };

                if (sourcePath.OverwritePolicy != TransferOverwritePolicy.Ignore)
                {
                    batch.Add(context.CallActivityAsync(
                        nameof(TransferFile),
                        transferFilePayload));
                }
                else
                {
                    logger.LogInformation(
                        "Skipping transfer for {SourcePath} due to OverwritePolicy.Ignore. CorrelationId: {CorrelationId}",
                        sourcePath.Path, input.CorrelationId);
                }

                if (batch.Count >= batchSize)
                {
                    await Task.WhenAll(batch);
                    batch.Clear();
                }
            }

            // Await any remaining tasks in the last batch
            if (batch.Count > 0)
            {
                await Task.WhenAll(batch);
            }

            // 3. Update activity log
            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferCompleted,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                });

            // 4. Finalize
            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload
                {
                    TransferId = input.TransferId,
                });

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

            await _activityLogService.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.TransferFailed,
                ActivityLog.Enums.ResourceType.FileTransfer,
                input.CaseId,
                input.TransferId.ToString(),
                input.TransferDirection.GetAlternateValue(),
                input.UserName,
                details: _activityLogService.ConvertToJsonDocument(ex.Message));

            throw;
        }
    }
}
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class TransferOrchestrator(IActivityLogService activityLogService)
{
    private readonly IActivityLogService _activityLogService = activityLogService;

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
            };

            await context.CallActivityAsync(
                nameof(IntializeTransfer),
                transferEntity);

            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferInitiated,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                });

            // 2. Fan-out: TransferFileActivity for each item
            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.InProgress,
                });

            var tasks = new List<Task>();

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

                tasks.Add(context.CallActivityAsync(
                    nameof(TransferFile),
                    transferFilePayload));
            }
            await Task.WhenAll(tasks);

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
            await _activityLogService.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.TransferFailed,
                ActivityLog.Enums.ResourceType.FileTransfer,
                input.CaseId,
                input.TransferId.ToString(),
                input.TransferDirection.GetAlternateValue(),
                input.UserName,
                details: _activityLogService.ConvertToJsonDocument(ex.Message));
        }
    }
}
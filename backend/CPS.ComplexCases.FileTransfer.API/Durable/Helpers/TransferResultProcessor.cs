using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;


public static class TransferResultProcessor
{
    public static async Task ProcessAsync(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        TransferResult[] results,
        TransferOrchestrationEvent telemetryEvent,
        bool isRetry = false)
    {
        var logger = context.CreateReplaySafeLogger(nameof(TransferResultProcessor));
        var batch = new TransferResultBatch { IsRetry = isRetry };

        foreach (var result in results)
        {
            if (result is null)
            {
                continue;
            }

            if (result.IsSkipped && result.SkippedItem != null)
            {
                batch.SkippedItems.Add(result.SkippedItem);
            }
            else if (result.IsSuccess && result.SuccessfulItem != null)
            {
                batch.SuccessfulItems.Add(result.SuccessfulItem);

                telemetryEvent.TotalFilesTransferred++;
                telemetryEvent.TotalBytesTransferred += result.SuccessfulItem.Size;
            }
            else if (!result.IsSuccess && result.FailedItem != null)
            {
                batch.FailedItems.Add(result.FailedItem);

                telemetryEvent.TotalFilesFailed++;
            }
            else
            {
                logger.LogWarning(
                    "Unclassifiable transfer result. IsSuccess={IsSuccess}, IsSkipped={IsSkipped}, HasSuccessfulItem={HasSuccessfulItem}, HasSkippedItem={HasSkippedItem}, HasFailedItem={HasFailedItem}",
                    result.IsSuccess,
                    result.IsSkipped,
                    result.SuccessfulItem is not null,
                    result.SkippedItem is not null,
                    result.FailedItem is not null);

                var failedItem = result.FailedItem ?? new TransferFailedItem
                {
                    SourcePath = result.SuccessfulItem?.SourcePath
                        ?? result.SkippedItem?.SourcePath
                        ?? "unknown",
                    Status = TransferItemStatus.Failed,
                    ErrorCode = TransferErrorCode.GeneralError,
                    ErrorMessage = "Unclassifiable transfer result"
                };

                batch.FailedItems.Add(failedItem);

                telemetryEvent.TotalFilesFailed++;
            }
        }

        if (!batch.HasItems)
        {
            return;
        }

        await context.Entities.CallEntityAsync(
            entityId,
            nameof(TransferEntityState.ApplyResultBatch),
            batch);
    }
}

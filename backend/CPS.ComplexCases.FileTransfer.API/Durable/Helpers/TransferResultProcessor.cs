using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

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
        foreach (var result in results)
        {
            if (result != null && result.IsSkipped && result.SkippedItem != null)
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.AddSkippedItem),
                    result.SkippedItem);
            }
            else if (result != null && result.IsSuccess && result.SuccessfulItem != null)
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

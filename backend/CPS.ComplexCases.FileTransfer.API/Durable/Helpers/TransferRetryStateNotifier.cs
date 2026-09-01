using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public static class TransferRetryStateNotifier
{
    public static Task WaitingForRetryAsync(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        int attempt,
        int maxAttempts,
        int retryingFileCount,
        int delaySeconds,
        DateTime nextRetryAt) =>
        context.Entities.CallEntityAsync(
            entityId,
            nameof(TransferEntityState.UpdateRetryState),
            new TransferRetryState
            {
                RetryAttempt = attempt,
                MaxRetryAttempts = maxAttempts,
                RetryingFileCount = retryingFileCount,
                RetryDelaySeconds = delaySeconds,
                NextRetryAt = nextRetryAt
            });

    public static Task RetryInProgressAsync(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        int attempt,
        int maxAttempts,
        int retryingFileCount,
        int delaySeconds) =>
        context.Entities.CallEntityAsync(
            entityId,
            nameof(TransferEntityState.UpdateRetryState),
            new TransferRetryState
            {
                RetryAttempt = attempt,
                MaxRetryAttempts = maxAttempts,
                RetryingFileCount = retryingFileCount,
                RetryDelaySeconds = delaySeconds,
                NextRetryAt = null
            });

    public static Task ClearAsync(TaskOrchestrationContext context, EntityInstanceId entityId) =>
        context.Entities.CallEntityAsync(entityId, nameof(TransferEntityState.ClearRetryState));
}

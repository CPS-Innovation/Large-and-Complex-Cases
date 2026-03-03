using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class PurgeTransferHistory(ILogger<PurgeTransferHistory> logger, IOptions<PurgeRetentionConfig> retentionConfig)
{
    private const string DeleteOperation = "Delete";
    private readonly ILogger<PurgeTransferHistory> _logger = logger;
    private readonly PurgeRetentionConfig _retentionConfig = retentionConfig.Value;

    [Function(nameof(PurgeTransferHistory))]
    public async Task Run(
        [TimerTrigger("%FileTransferPurgeRetentionSchedule%")] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_retentionConfig.RetentionDays);
        _logger.LogInformation(
            "PurgeTransferHistory started. Retention: {RetentionDays} days, cutoff: {Cutoff}",
            _retentionConfig.RetentionDays, cutoff);

        var terminalStatuses = new[]
        {
            OrchestrationRuntimeStatus.Completed,
            OrchestrationRuntimeStatus.Failed,
            OrchestrationRuntimeStatus.Terminated
        };

        // 1. Query instances that are terminal and older than cutoff
        var query = new OrchestrationQuery
        {
            CreatedTo = cutoff,
            Statuses = terminalStatuses
        };

        var entityDeleteCount = 0;
        await foreach (var instance in client.GetAllInstancesAsync(query).WithCancellation(cancellationToken))
        {
            if (string.IsNullOrEmpty(instance.InstanceId))
                continue;

            try
            {
                var entityId = new EntityInstanceId(nameof(TransferEntityState), instance.InstanceId);
                await client.Entities.SignalEntityAsync(entityId, DeleteOperation, null, null, cancellationToken);
                entityDeleteCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to signal entity delete for instance {InstanceId}. Continuing with purge.",
                    instance.InstanceId);
            }
        }

        _logger.LogInformation(
            "Signalled delete for {Count} transfer entities older than {Cutoff}",
            entityDeleteCount, cutoff);

        // 2. Purge orchestration instance history
        try
        {
            var filter = new PurgeInstancesFilter(null, cutoff, terminalStatuses);
            var result = await client.PurgeAllInstancesAsync(filter, cancellationToken);
            _logger.LogInformation(
                "Purged {Count} orchestration instances older than {Cutoff}",
                result.PurgedInstanceCount, cutoff);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to purge orchestration instances. Entity state was still signalled for deletion.");
        }
    }
}

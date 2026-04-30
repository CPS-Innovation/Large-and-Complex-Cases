using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class CleanupStaleManageMaterialsOperations(
    ILogger<CleanupStaleManageMaterialsOperations> logger,
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    IOptions<ManageMaterialsCleanupConfig> cleanupConfig)
{
    private static readonly OrchestrationRuntimeStatus[] TerminalStatuses =
    [
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated,
    ];

    private readonly ILogger<CleanupStaleManageMaterialsOperations> _logger = logger;
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly ManageMaterialsCleanupConfig _config = cleanupConfig.Value;

    [Function(nameof(CleanupStaleManageMaterialsOperations))]
    public async Task Run(
        [TimerTrigger("%ManageMaterialsCleanupSchedule%")] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient orchestrationClient,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var maxAge = TimeSpan.FromHours(_config.MaxAgeHours);

        _logger.LogInformation(
            "CleanupStaleManageMaterialsOperations started. MaxAgeHours: {MaxAgeHours}",
            _config.MaxAgeHours);

        var operations = await _caseActiveManageMaterialsService.GetAllActiveOperationsAsync();
        var deletedCount = 0;

        foreach (var operation in operations)
        {
            try
            {
                var instance = await orchestrationClient.GetInstancesAsync(
                    operation.Id.ToString(), cancellation: cancellationToken);

                var isTerminal = instance != null && TerminalStatuses.Contains(instance.RuntimeStatus);
                var isNotFound = instance == null;
                var isAged = now - operation.CreatedAt > maxAge;

                // Delete if the orchestration is in a terminal state.
                // Also delete if the instance is not found and the row is older than maxAge —
                // this covers cases where durable history has been purged for a genuinely finished
                // operation, while protecting rows for recently-started operations whose instance
                // may not yet be visible.
                if (isTerminal || (isNotFound && isAged))
                {
                    if (isNotFound && isAged)
                    {
                        _logger.LogWarning(
                            "Deleting manage materials row {OperationId} for CaseId {CaseId}: orchestration instance not found and row age {AgeHours:F1}h exceeds max {MaxAgeHours}h.",
                            operation.Id, operation.CaseId, (now - operation.CreatedAt).TotalHours, _config.MaxAgeHours);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Deleting manage materials row {OperationId} for CaseId {CaseId}: orchestration status is {Status}.",
                            operation.Id, operation.CaseId, instance!.RuntimeStatus.ToString());
                    }

                    await _caseActiveManageMaterialsService.DeleteOperationAsync(operation.Id);
                    deletedCount++;
                }
                else if (isNotFound)
                {
                    _logger.LogWarning(
                        "Manage materials row {OperationId} for CaseId {CaseId} has no durable instance but is only {AgeHours:F1}h old — retaining until age exceeds {MaxAgeHours}h.",
                        operation.Id, operation.CaseId, (now - operation.CreatedAt).TotalHours, _config.MaxAgeHours);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to process manage materials row {OperationId} for CaseId {CaseId}. Skipping.",
                    operation.Id, operation.CaseId);
            }
        }

        _logger.LogInformation(
            "CleanupStaleManageMaterialsOperations completed. Deleted {DeletedCount} stale row(s).",
            deletedCount);
    }
}

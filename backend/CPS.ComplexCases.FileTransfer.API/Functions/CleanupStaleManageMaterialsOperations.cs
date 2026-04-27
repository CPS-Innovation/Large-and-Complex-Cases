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
                // Age-based safety net: delete without consulting durable
                if (now - operation.CreatedAt > maxAge)
                {
                    _logger.LogWarning(
                        "Deleting stale manage materials row {OperationId} for CaseId {CaseId}: age {AgeHours:F1}h exceeds max {MaxAgeHours}h.",
                        operation.Id, operation.CaseId, (now - operation.CreatedAt).TotalHours, _config.MaxAgeHours);

                    await _caseActiveManageMaterialsService.DeleteOperationAsync(operation.Id);
                    deletedCount++;
                    continue;
                }

                // Status-based cleanup: delete if orchestration is in a terminal state or not found
                var instance = await orchestrationClient.GetInstancesAsync(
                    operation.Id.ToString(), cancellation: cancellationToken);

                if (instance == null || TerminalStatuses.Contains(instance.RuntimeStatus))
                {
                    _logger.LogInformation(
                        "Deleting manage materials row {OperationId} for CaseId {CaseId}: orchestration status is {Status}.",
                        operation.Id, operation.CaseId, instance?.RuntimeStatus.ToString() ?? "NotFound");

                    await _caseActiveManageMaterialsService.DeleteOperationAsync(operation.Id);
                    deletedCount++;
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

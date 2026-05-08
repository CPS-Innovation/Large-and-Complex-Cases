using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Services;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class RemoveActiveManageMaterialsOperation(
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    ILogger<RemoveActiveManageMaterialsOperation> logger)
{
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly ILogger<RemoveActiveManageMaterialsOperation> _logger = logger;

    [Function(nameof(RemoveActiveManageMaterialsOperation))]
    public async Task Run([ActivityTrigger] Guid operationId)
    {
        _logger.LogInformation("Removing manage materials operation row {OperationId}", operationId);
        try
        {
            await _caseActiveManageMaterialsService.DeleteOperationAsync(operationId);
            _logger.LogInformation("Successfully removed manage materials operation row {OperationId}", operationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove manage materials operation row {OperationId}. Row may require manual cleanup.", operationId);
            // Do not rethrow — failure to delete the row must not fail the orchestration.
            // The timer-triggered cleanup job handles stale rows.
        }
    }
}

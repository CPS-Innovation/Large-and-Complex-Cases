using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class WriteCopyActivityLog(
    IActivityLogService activityLogService,
    ILogger<WriteCopyActivityLog> logger,
    IInitializationHandler initializationHandler)
{
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly ILogger<WriteCopyActivityLog> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(WriteCopyActivityLog))]
    public async Task Run([ActivityTrigger] WriteCopyActivityLogPayload payload)
    {
        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId);

        var successfulKeySet = new HashSet<string>(payload.SuccessfulSourceKeys, StringComparer.OrdinalIgnoreCase);

        foreach (var operation in payload.OriginalOperations)
        {
            try
            {
                if (string.Equals(operation.Type, "Folder", StringComparison.OrdinalIgnoreCase))
                {
                    var hasCopied = successfulKeySet.Any(key =>
                        key.StartsWith(operation.SourcePath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(key, operation.SourcePath, StringComparison.OrdinalIgnoreCase));

                    if (hasCopied)
                    {
                        var details = new { sourcePath = operation.SourcePath }.SerializeToJsonDocument(_logger);
                        await _activityLogService.CreateActivityLogAsync(
                            actionType: ActionType.FolderCopied,
                            resourceType: ResourceType.Material,
                            caseId: payload.CaseId,
                            resourceId: operation.SourcePath,
                            resourceName: Path.GetFileName(operation.SourcePath.TrimEnd('/')),
                            userName: payload.UserName,
                            details: details);
                    }
                }
                else
                {
                    if (successfulKeySet.Contains(operation.SourcePath))
                    {
                        var details = new { sourcePath = operation.SourcePath }.SerializeToJsonDocument(_logger);
                        await _activityLogService.CreateActivityLogAsync(
                            actionType: ActionType.MaterialCopied,
                            resourceType: ResourceType.Material,
                            caseId: payload.CaseId,
                            resourceId: operation.SourcePath,
                            resourceName: Path.GetFileName(operation.SourcePath),
                            userName: payload.UserName,
                            details: details);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write activity log for copy operation {SourcePath}. CaseId: {CaseId}",
                    operation.SourcePath, payload.CaseId);
            }
        }
    }
}

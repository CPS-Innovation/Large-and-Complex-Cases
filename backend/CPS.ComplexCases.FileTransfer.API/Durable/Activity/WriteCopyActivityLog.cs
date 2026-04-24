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

        var items = payload.OriginalOperations.Select(op =>
        {
            var isCopied = string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase)
                ? successfulKeySet.Any(key =>
                    key.StartsWith(op.SourcePath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(key, op.SourcePath, StringComparison.OrdinalIgnoreCase))
                : successfulKeySet.Contains(op.SourcePath);

            return new
            {
                sourcePath = op.SourcePath,
                outcome = isCopied ? "Copied" : "NotCopied",
                type = op.Type
            };
        }).ToList();

        var copiedOps = payload.OriginalOperations
            .Zip(items, (op, item) => (op, item))
            .Where(x => x.item.outcome == "Copied")
            .Select(x => x.op)
            .ToList();

        if (copiedOps.Count == 0)
        {
            _logger.LogInformation("No operations were successfully copied for CaseId: {CaseId}. Skipping activity log.", payload.CaseId);
            return;
        }

        try
        {
            var hasFolder = copiedOps.Any(op => string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase));
            var hasMaterial = copiedOps.Any(op => !string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase));

            var (actionType, resourceType) = (hasFolder, hasMaterial) switch
            {
                (true, true) => (ActionType.FolderAndMaterialCopied, ResourceType.Material),
                (true, false) => (ActionType.FolderCopied, ResourceType.NetAppFolder),
                _ => (ActionType.MaterialCopied, ResourceType.Material)
            };

            var details = new { items }.SerializeToJsonDocument(_logger);

            await _activityLogService.CreateActivityLogAsync(
                actionType,
                resourceType,
                payload.CaseId,
                payload.CaseId.ToString(),
                null,
                payload.UserName,
                details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write batch copy activity log for CaseId: {CaseId}.", payload.CaseId);
        }
    }
}

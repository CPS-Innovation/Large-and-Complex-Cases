using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class WriteMoveActivityLog(
    IActivityLogService activityLogService,
    ILogger<WriteMoveActivityLog> logger,
    IInitializationHandler initializationHandler)
{
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly ILogger<WriteMoveActivityLog> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(WriteMoveActivityLog))]
    public async Task Run([ActivityTrigger] WriteMoveActivityLogPayload payload)
    {
        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId);

        var successfulKeySet = new HashSet<string>(payload.SuccessfulSourceKeys, StringComparer.OrdinalIgnoreCase);

        var items = payload.OriginalOperations.Select(op => new
        {
            sourcePath = op.SourcePath,
            destinationPath = GetDestinationPath(op),
            outcome = DetermineOutcome(op, successfulKeySet),
            type = op.Type
        }).ToList();

        var reportableOps = payload.OriginalOperations
            .Zip(items, (op, item) => (op, item))
            .Where(x => x.item.outcome != "NotMoved")
            .Select(x => x.op)
            .ToList();

        if (reportableOps.Count == 0)
        {
            _logger.LogInformation("No operations were successfully moved for CaseId: {CaseId}. Skipping activity log.", payload.CaseId);
            return;
        }

        try
        {
            var hasFolder = reportableOps.Any(op => string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase));
            var hasMaterial = reportableOps.Any(op => !string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase));

            var (actionType, resourceType) = (hasFolder, hasMaterial) switch
            {
                (true, true) => (ActionType.FolderAndMaterialMoved, ResourceType.Material),
                (true, false) => (ActionType.FolderMoved, ResourceType.NetAppFolder),
                _ => (ActionType.MaterialMoved, ResourceType.Material)
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
            _logger.LogError(ex, "Failed to write batch move activity log for CaseId: {CaseId}.", payload.CaseId);
        }
    }

    private static string DetermineOutcome(MoveBatchOriginalOperation op, HashSet<string> successfulKeySet)
    {
        if (!string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase) || op.ExpectedSourceKeys.Count == 0)
            return successfulKeySet.Contains(op.SourcePath) ? "Moved" : "NotMoved";

        var successCount = op.ExpectedSourceKeys.Count(key => successfulKeySet.Contains(key));
        if (successCount == op.ExpectedSourceKeys.Count) return "Moved";
        if (successCount > 0) return "Partial";
        return "NotMoved";
    }

    private static string GetDestinationPath(MoveBatchOriginalOperation op) =>
        string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase)
            ? op.DestinationPrefix
            : op.DestinationPrefix + Path.GetFileName(op.SourcePath);
}

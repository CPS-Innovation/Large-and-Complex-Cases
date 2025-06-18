using System.Text.Json;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.Data.Dtos;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.ActivityLog.Services;

public class ActivityLogService(IActivityLogRepository activityLogRepository, ILogger<ActivityLogService> logger) : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository = activityLogRepository;
    private readonly ILogger<ActivityLogService> _logger = logger;

    public async Task CreateActivityLogAsync(ActionType actionType, ResourceType resourceType, int caseId, string resourceId, string? resourceName, string? userName, JsonDocument? details = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogWarning("Attempted to create activity log with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(resourceId));
        }

        _logger.LogInformation("Creating activity log for {ResourceType} {ResourceId}", resourceType, resourceId);
        var activityLog = new Data.Entities.ActivityLog
        {
            ActionType = actionType.GetAlternateValue(),
            ResourceType = resourceType.ToString(),
            CaseId = caseId,
            ResourceId = resourceId,
            ResourceName = resourceName,
            UserName = userName,
            Details = details,
            Timestamp = DateTime.UtcNow,
            Description = SetDescription(actionType, resourceName)
        };

        await _activityLogRepository.AddAsync(activityLog);
    }

    public Task<Data.Entities.ActivityLog?> GetActivityLogByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to get activity log with empty Guid.");
            throw new ArgumentException("Id cannot be Guid.Empty.", nameof(id));
        }

        _logger.LogInformation("Getting activity log by ID {Id}", id);

        return _activityLogRepository.GetByIdAsync(id);
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsAsync(ActivityLogFilterDto filter)
    {
        _logger.LogInformation("Getting activity logs with filter {@Filter}", filter);

        return _activityLogRepository.GetByFilterAsync(filter);
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsByResourceIdAsync(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogWarning("Attempted to get activity logs with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(resourceId));
        }

        _logger.LogInformation("Getting activity logs for {ResourceId}", resourceId);

        return _activityLogRepository.GetByResourceIdAsync(resourceId);
    }

    public Task<Data.Entities.ActivityLog?> UpdateActivityLogAsync(Data.Entities.ActivityLog activityLog)
    {
        if (string.IsNullOrWhiteSpace(activityLog.ResourceId))
        {
            _logger.LogWarning("Attempted to update activity log with null or empty resourceId.");
            throw new ArgumentException("ResourceId cannot be null or empty.", nameof(activityLog.ResourceId));
        }
        if (string.IsNullOrWhiteSpace(activityLog.ResourceType))
        {
            _logger.LogWarning("Attempted to update activity log with null or empty resourceType.");
            throw new ArgumentException("ResourceType cannot be null or empty.", nameof(activityLog.ResourceType));
        }
        if (activityLog.Id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to update activity log with empty Guid.");
            throw new ArgumentException("ActivityLog Id cannot be Guid.Empty.", nameof(activityLog.Id));
        }

        _logger.LogInformation("Updating activity log for case {ResourceType} {ResourceId}", activityLog.ResourceType, activityLog.ResourceId);

        return _activityLogRepository.UpdateAsync(activityLog);
    }

    public JsonDocument? ConvertToJsonDocument<T>(T data)
    {
        try
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(data));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error converting data to JsonDocument");
            return null;
        }
    }

    private static string SetDescription(ActionType actionType, string? resourceName)
    {
        return actionType switch
        {
            ActionType.ConnectionToEgress => $"Connected to Egress workspace {resourceName}",
            ActionType.ConnectionToNetApp => $"Connected to NetApp folder {resourceName}",
            ActionType.TransferInitiated => $"Transfer initiated between {resourceName}",
            ActionType.TransferCompleted => $"Transfer completed between {resourceName}",
            ActionType.TransferFailed => $"Transfer failed between {resourceName}",
            _ => $"Performed action {actionType} on resource {resourceName}"
        };
    }
}

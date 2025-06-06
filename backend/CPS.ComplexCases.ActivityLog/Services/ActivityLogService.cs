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
        _logger.LogInformation("Creating audit log for {ResourceType} {ResourceId}", resourceType, resourceId);
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {ResourceType} {ResourceId}", resourceType, resourceId);
            throw;
        }
    }

    public Task<Data.Entities.ActivityLog?> GetActivityLogByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting audit log by ID {Id}", id);
        try
        {
            return _activityLogRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log by ID {Id}", id);
            throw;
        }
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsAsync(ActivityLogFilterDto filter)
    {
        _logger.LogInformation("Getting audit logs with filter {@Filter}", filter);
        try
        {
            return _activityLogRepository.GetByFilterAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs with filter {@Filter}", filter);
            throw;
        }
    }

    public Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsByResourceIdAsync(string resourceId)
    {
        _logger.LogInformation("Getting audit logs for {ResourceId}", resourceId);
        try
        {
            return _activityLogRepository.GetByResourceIdAsync(resourceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for {ResourceId}", resourceId);
            throw;
        }
    }

    public Task<Data.Entities.ActivityLog?> UpdateActivityLogAsync(Data.Entities.ActivityLog auditLog)
    {
        _logger.LogInformation("Updating audit log for case {ResourceType} {ResourceId}", auditLog.ResourceType, auditLog.ResourceId);
        try
        {
            return _activityLogRepository.UpdateAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit log for case {ResourceType} {ResourceId}", auditLog.ResourceType, auditLog.ResourceId);
            throw;
        }
    }

    public Task<JsonDocument?> ConvertToJsonDocument<T>(T data)
    {
        return Task.Run(() =>
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
        });
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

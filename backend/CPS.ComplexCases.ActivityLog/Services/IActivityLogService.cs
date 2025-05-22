using System.Text.Json;
using CPS.ComplexCases.ActivityLog.Enums;

namespace CPS.ComplexCases.ActivityLog.Services;

public interface IActivityLogService
{
    Task CreateActivityLogAsync(ActionType actionType, ResourceType resourceType, string resourceId, string? resourceName, string? userName, JsonDocument? details = null);
    Task<Data.Entities.ActivityLog?> GetActivityLogByIdAsync(Guid id);
    Task<Data.Entities.ActivityLog?> UpdateActivityLogAsync(Data.Entities.ActivityLog auditLog);
    Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsByResourceIdAsync(string resourceId);
}
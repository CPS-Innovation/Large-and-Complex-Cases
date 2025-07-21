using System.Text.Json;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Models.Responses;
using CPS.ComplexCases.Data.Dtos;

namespace CPS.ComplexCases.ActivityLog.Services;

public interface IActivityLogService
{
    Task CreateActivityLogAsync(ActionType actionType, ResourceType resourceType, int caseId, string resourceId, string? resourceName, string? userName, JsonDocument? details = null);
    Task<Data.Entities.ActivityLog?> GetActivityLogByIdAsync(Guid id);
    Task<Data.Entities.ActivityLog?> UpdateActivityLogAsync(Data.Entities.ActivityLog auditLog);
    Task<IEnumerable<Data.Entities.ActivityLog>> GetActivityLogsByResourceIdAsync(string resourceId);
    Task<ActivityLogsResponse> GetActivityLogsAsync(ActivityLogFilterDto filter);
    string GenerateFileDetailsCsvAsync(Data.Entities.ActivityLog activityLog);
}
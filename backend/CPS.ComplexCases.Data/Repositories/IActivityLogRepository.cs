using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Repositories;

public interface IActivityLogRepository
{
    Task<ActivityLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivityLog>> GetByResourceIdAsync(string resourceId);
    Task<ActivityLog> AddAsync(ActivityLog activityLog);
    Task<ActivityLog?> UpdateAsync(ActivityLog activityLog);
}
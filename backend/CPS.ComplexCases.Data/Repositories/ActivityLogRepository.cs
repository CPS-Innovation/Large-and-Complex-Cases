using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Repositories;

public class ActivityLogRepository(ApplicationDbContext dbContext) : IActivityLogRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ActivityLog?> GetByIdAsync(Guid id)
    {
        return await _dbContext.ActivityLogs.FindAsync(id);
    }

    public async Task<IEnumerable<ActivityLog>> GetByResourceIdAsync(string resourceId)
    {
        return await _dbContext.ActivityLogs
                     .Where(a => a.ResourceId == resourceId)
                     .ToListAsync();
    }

    public async Task<ActivityLog> AddAsync(ActivityLog activityLog)
    {
        await _dbContext.ActivityLogs.AddAsync(activityLog);
        await _dbContext.SaveChangesAsync();
        return activityLog;
    }

    public async Task<ActivityLog?> UpdateAsync(ActivityLog activityLog)
    {
        var existingAuditLog = await GetByIdAsync(activityLog.Id);

        if (existingAuditLog == null)
        {
            return null;
        }

        _dbContext.Entry(existingAuditLog).CurrentValues.SetValues(activityLog);
        await _dbContext.SaveChangesAsync();
        return existingAuditLog;
    }
}
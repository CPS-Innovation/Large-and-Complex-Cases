using CPS.ComplexCases.Data.Dtos;
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
        var existingActivityLog = await GetByIdAsync(activityLog.Id);

        if (existingActivityLog == null)
        {
            return null;
        }

        _dbContext.Entry(existingActivityLog).CurrentValues.SetValues(activityLog);
        await _dbContext.SaveChangesAsync();
        return existingActivityLog;
    }

    public async Task<ActivityLogResultsDto> GetByFilterAsync(ActivityLogFilterDto filter)
    {
        IQueryable<ActivityLog> query = _dbContext.ActivityLogs.AsNoTracking();

        if (filter.FromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= filter.FromDate.Value.ToUniversalTime());
        }
        if (filter.ToDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= filter.ToDate.Value.ToUniversalTime());
        }
        if (!string.IsNullOrEmpty(filter.Username))
        {
            query = query.Where(a => a.UserName == filter.Username);
        }
        if (!string.IsNullOrEmpty(filter.ActionType))
        {
            query = query.Where(a => a.ActionType == filter.ActionType);
        }
        if (!string.IsNullOrEmpty(filter.ResourceType))
        {
            query = query.Where(a => a.ResourceType == filter.ResourceType);
        }
        if (!string.IsNullOrEmpty(filter.ResourceId))
        {
            query = query.Where(a => a.ResourceId == filter.ResourceId);
        }

        var totalCount = await query.CountAsync();

        var results = await query.OrderByDescending(x => x.Timestamp)
                        .ThenByDescending(x => x.Id)
                        .Skip(filter.Skip)
                        .Take(filter.Take)
                        .ToListAsync();

        return new ActivityLogResultsDto
        {
            Logs = results,
            TotalCount = totalCount,
            Skip = filter.Skip,
            Take = filter.Take
        };
    }
}
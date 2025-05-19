using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Repositories;

public class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return null;// await _dbContext.AuditLogs.FindAsync(id);
    }

    public async Task<IEnumerable<AuditLog>> GetByCaseIdAsync(int caseId)
    {
        return null; // await _dbContext.AuditLogs
                     //.Where(a => a.CaseId == caseId)
                     // .ToListAsync();
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        // await _dbContext.AuditLogs.AddAsync(auditLog);
        // await _dbContext.SaveChangesAsync();
        return null;// auditLog;
    }

    public async Task<AuditLog?> UpdateAsync(AuditLog auditLog)
    {
        var existingAuditLog = await GetByIdAsync(auditLog.Id);

        if (existingAuditLog == null)
        {
            return null;
        }

        _dbContext.Entry(existingAuditLog).CurrentValues.SetValues(auditLog);
        await _dbContext.SaveChangesAsync();
        return existingAuditLog;
    }
}
using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetByCaseIdAsync(int caseId);
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task<AuditLog?> UpdateAsync(AuditLog auditLog);
}
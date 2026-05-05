using System.Data;
using System.Text.Json;
using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Repositories;

public class CaseActiveManageMaterialsRepository(ApplicationDbContext dbContext) : ICaseActiveManageMaterialsRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task DeleteAsync(Guid id)
    {
        var operation = await _dbContext.CaseActiveManageMaterialsOperations.FindAsync(id);
        if (operation != null)
        {
            _dbContext.CaseActiveManageMaterialsOperations.Remove(operation);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetActiveOperationsForCaseAsync(int caseId)
    {
        return await _dbContext.CaseActiveManageMaterialsOperations
            .Where(x => x.CaseId == caseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetAllActiveOperationsAsync()
    {
        return await _dbContext.CaseActiveManageMaterialsOperations.ToListAsync();
    }

    private async Task<bool> HasConflictingOperationAsync(int caseId, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths)
    {
        var activeOperations = await _dbContext.CaseActiveManageMaterialsOperations
            .Where(x => x.CaseId == caseId)
            .ToListAsync();

        if (!activeOperations.Any())
            return false;

        var incomingSourceList = sourcePaths.ToList();
        var incomingDestList = destinationPaths.ToList();

        foreach (var operation in activeOperations)
        {
            var existingSourcePaths = JsonSerializer.Deserialize<List<string>>(operation.SourcePaths) ?? [];
            var existingDestPaths = operation.DestinationPaths != null
                ? JsonSerializer.Deserialize<List<string>>(operation.DestinationPaths) ?? []
                : [];

            // A conflict exists if any incoming source path overlaps with an existing source or destination path
            foreach (var incomingSource in incomingSourceList)
            {
                if (existingSourcePaths.Any(existing => PathsOverlap(incomingSource, existing)))
                    return true;

                if (existingDestPaths.Any(existing => PathsOverlap(incomingSource, existing)))
                    return true;
            }

            // Or if any incoming destination overlaps with an existing source or destination path
            foreach (var incomingDest in incomingDestList)
            {
                if (existingSourcePaths.Any(existing => PathsOverlap(incomingDest, existing)))
                    return true;

                if (existingDestPaths.Any(existing => PathsOverlap(incomingDest, existing)))
                    return true;
            }
        }

        return false;
    }

    public async Task<bool> CheckConflictAndInsertAsync(
        CaseActiveManageMaterialsOperation operation,
        IEnumerable<string> sourcePaths,
        IEnumerable<string> destinationPaths)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (await HasConflictingOperationAsync(operation.CaseId, sourcePaths, destinationPaths))
            {
                await tx.RollbackAsync();
                return false;
            }

            await _dbContext.CaseActiveManageMaterialsOperations.AddAsync(operation);
            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        });
    }

    private static bool PathsOverlap(string pathA, string pathB)
    {
        var a = pathA.TrimEnd('/');
        var b = pathB.TrimEnd('/');

        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
            || a.StartsWith(b + "/", StringComparison.OrdinalIgnoreCase)
            || b.StartsWith(a + "/", StringComparison.OrdinalIgnoreCase);
    }
}

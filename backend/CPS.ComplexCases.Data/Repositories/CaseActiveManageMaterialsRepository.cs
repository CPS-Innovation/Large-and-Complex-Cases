using System.Text.Json;
using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Repositories;

public class CaseActiveManageMaterialsRepository(ApplicationDbContext dbContext) : ICaseActiveManageMaterialsRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task InsertAsync(CaseActiveManageMaterialsOperation operation)
    {
        await _dbContext.CaseActiveManageMaterialsOperations.AddAsync(operation);
        await _dbContext.SaveChangesAsync();
    }

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

    public async Task<bool> HasConflictingOperationAsync(int caseId, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths)
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

            // Or if any incoming destination overlaps with an existing source path
            foreach (var incomingDest in incomingDestList)
            {
                if (existingSourcePaths.Any(existing => PathsOverlap(incomingDest, existing)))
                    return true;
            }
        }

        return false;
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

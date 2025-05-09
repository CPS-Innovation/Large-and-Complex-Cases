using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Repositories
{

  public class CaseMetadataRepository : ICaseMetadataRepository
  {
    private readonly ApplicationDbContext _dbContext;

    public CaseMetadataRepository(ApplicationDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<CaseMetadata?> GetByCaseIdAsync(int caseId)
    {
      var metadata = await _dbContext.CaseMetadata.FindAsync(caseId);
      return metadata;
    }

    public async Task<CaseMetadata> AddAsync(CaseMetadata metadata)
    {
      await _dbContext.CaseMetadata.AddAsync(metadata);
      await _dbContext.SaveChangesAsync();
      return metadata;
    }

    public async Task<CaseMetadata?> UpdateAsync(CaseMetadata metadata)
    {
      var existingMetadata = await _dbContext.CaseMetadata.FindAsync(metadata.CaseId);

      if (existingMetadata == null)
      {
        return null;
      }

      _dbContext.Entry(existingMetadata).CurrentValues.SetValues(metadata);
      await _dbContext.SaveChangesAsync();
      return existingMetadata;
    }

    public async Task<IEnumerable<CaseMetadata>> GetByCaseIdsAsync(IEnumerable<int> caseIds)
    {
      return await _dbContext.CaseMetadata
          .Where(m => caseIds.Contains(m.CaseId))
          .ToListAsync();
    }

    public async Task<IEnumerable<CaseMetadata>> GetByEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds)
    {
      return await _dbContext.CaseMetadata
          .Where(m => egressWorkspaceIds.Contains(m.EgressWorkspaceId))
          .ToListAsync();
    }

    public async Task<IEnumerable<CaseMetadata>> GetByNetAppFolderPathsAsync(IEnumerable<string> netAppFolderPaths)
    {
      return await _dbContext.CaseMetadata
          .Where(m => netAppFolderPaths.Contains(m.NetappFolderPath))
          .ToListAsync();
    }
  }
}
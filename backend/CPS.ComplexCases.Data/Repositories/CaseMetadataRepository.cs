using CPS.ComplexCases.Data.Entities;

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
      _dbContext.CaseMetadata.Add(metadata);
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
  }
}
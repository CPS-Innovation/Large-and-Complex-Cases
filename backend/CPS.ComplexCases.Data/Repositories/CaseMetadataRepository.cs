using CPS.ComplexCases.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        public async Task<CaseMetadata?> GetByActiveTransferIdAsync(Guid activeTransferId)
        {
            return await _dbContext.CaseMetadata
                .FirstOrDefaultAsync(m => m.ActiveTransferId == activeTransferId);
        }

        public Task<bool> TrySetEgressConnectionAsync(int caseId, string workspaceId, string? workspaceName) =>
            TrySetConnectionAsync(
                caseId,
                () => _dbContext.CaseMetadata
                    .Where(m => m.CaseId == caseId && m.EgressWorkspaceId == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.EgressWorkspaceId, workspaceId)
                        .SetProperty(m => m.EgressWorkspaceName, workspaceName)),
                () => new CaseMetadata
                {
                    CaseId = caseId,
                    EgressWorkspaceId = workspaceId,
                    EgressWorkspaceName = workspaceName
                });

        public Task<bool> TrySetNetAppConnectionAsync(int caseId, string folderPath, string? bucketName) =>
            TrySetConnectionAsync(
                caseId,
                () => _dbContext.CaseMetadata
                    .Where(m => m.CaseId == caseId && m.NetappFolderPath == null)
                    .ExecuteUpdateAsync(s =>
                    {
                        s.SetProperty(m => m.NetappFolderPath, folderPath);

                        // A null bucket must not wipe an already-persisted value.
                        if (!string.IsNullOrEmpty(bucketName))
                        {
                            s.SetProperty(m => m.NetappBucketName, bucketName);
                        }
                    }),
                () => new CaseMetadata
                {
                    CaseId = caseId,
                    NetappFolderPath = folderPath,
                    NetappBucketName = bucketName
                });

        private async Task<bool> TrySetConnectionAsync(int caseId, Func<Task<int>> conditionalUpdate, Func<CaseMetadata> createNew)
        {
            try
            {
                if (await conditionalUpdate() > 0)
                {
                    return true;
                }

                if (await _dbContext.CaseMetadata.AsNoTracking().AnyAsync(m => m.CaseId == caseId))
                {
                    return false;
                }

                var newMetadata = createNew();
                await _dbContext.CaseMetadata.AddAsync(newMetadata);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _dbContext.Entry(newMetadata).State = EntityState.Detached;
                    return false;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return false;
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    }
}

using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.API.Integration.Tests.Database;

[Collection("Integration Tests")]
public class DatabaseIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public DatabaseIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryAll()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured - set ConnectionStrings__CaseManagementDatastoreConnection");

        // Act
        var count = await _fixture.DbContext!.CaseMetadata.CountAsync();

        // Assert
        Assert.True(count >= 0, "Should be able to query CaseMetadata table");
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryByCaseId()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingMetadata = await _fixture.DbContext!.CaseMetadata.FirstOrDefaultAsync();
        Skip.If(existingMetadata == null, "No CaseMetadata records exist in database");

        // Act
        var metadata = await _fixture.DbContext.CaseMetadata
            .FirstOrDefaultAsync(c => c.CaseId == existingMetadata!.CaseId);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(existingMetadata!.CaseId, metadata.CaseId);
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryByEgressWorkspaceId()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingMetadata = await _fixture.DbContext!.CaseMetadata
            .FirstOrDefaultAsync(c => c.EgressWorkspaceId != null);
        Skip.If(existingMetadata == null, "No CaseMetadata records with EgressWorkspaceId exist");

        // Act
        var results = await _fixture.DbContext.CaseMetadata
            .Where(c => c.EgressWorkspaceId == existingMetadata!.EgressWorkspaceId)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingMetadata!.EgressWorkspaceId, r.EgressWorkspaceId));
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryByNetAppFolderPath()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingMetadata = await _fixture.DbContext!.CaseMetadata
            .FirstOrDefaultAsync(c => c.NetappFolderPath != null);
        Skip.If(existingMetadata == null, "No CaseMetadata records with NetappFolderPath exist");

        // Act
        var results = await _fixture.DbContext.CaseMetadata
            .Where(c => c.NetappFolderPath == existingMetadata!.NetappFolderPath)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingMetadata!.NetappFolderPath, r.NetappFolderPath));
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryByActiveTransferId()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingMetadata = await _fixture.DbContext!.CaseMetadata
            .FirstOrDefaultAsync(c => c.ActiveTransferId != null);
        Skip.If(existingMetadata == null, "No CaseMetadata records with ActiveTransferId exist");

        // Act
        var result = await _fixture.DbContext.CaseMetadata
            .FirstOrDefaultAsync(c => c.ActiveTransferId == existingMetadata!.ActiveTransferId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingMetadata!.ActiveTransferId, result.ActiveTransferId);
    }

    [SkippableFact]
    public async Task CaseMetadata_CanQueryMultipleByCaseIds()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingMetadata = await _fixture.DbContext!.CaseMetadata
            .Take(3)
            .ToListAsync();
        Skip.If(existingMetadata.Count == 0, "No CaseMetadata records exist");

        var caseIds = existingMetadata.Select(m => m.CaseId).ToList();

        // Act
        var results = await _fixture.DbContext.CaseMetadata
            .Where(c => caseIds.Contains(c.CaseId))
            .ToListAsync();

        // Assert
        Assert.Equal(caseIds.Count, results.Count);
        Assert.All(results, r => Assert.Contains(r.CaseId, caseIds));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryAll()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Act
        var count = await _fixture.DbContext!.ActivityLogs.CountAsync();

        // Assert
        Assert.True(count >= 0, "Should be able to query ActivityLogs table");
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryById()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs.FirstOrDefaultAsync();
        Skip.If(existingLog == null, "No ActivityLog records exist in database");

        // Act
        var log = await _fixture.DbContext.ActivityLogs.FindAsync(existingLog!.Id);

        // Assert
        Assert.NotNull(log);
        Assert.Equal(existingLog.Id, log.Id);
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByResourceId()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs
            .FirstOrDefaultAsync(a => a.ResourceId != null);
        Skip.If(existingLog == null, "No ActivityLog records with ResourceId exist");

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.ResourceId == existingLog!.ResourceId)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingLog!.ResourceId, r.ResourceId));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByActionType()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs
            .FirstOrDefaultAsync(a => a.ActionType != null);
        Skip.If(existingLog == null, "No ActivityLog records with ActionType exist");

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.ActionType == existingLog!.ActionType)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingLog!.ActionType, r.ActionType));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByUsername()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs
            .FirstOrDefaultAsync(a => a.UserName != null);
        Skip.If(existingLog == null, "No ActivityLog records with UserName exist");

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.UserName == existingLog!.UserName)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingLog!.UserName, r.UserName));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByResourceType()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs
            .FirstOrDefaultAsync(a => a.ResourceType != null);
        Skip.If(existingLog == null, "No ActivityLog records with ResourceType exist");

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.ResourceType == existingLog!.ResourceType)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingLog!.ResourceType, r.ResourceType));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByCaseId()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs
            .FirstOrDefaultAsync(a => a.CaseId != null);
        Skip.If(existingLog == null, "No ActivityLog records with CaseId exist");

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.CaseId == existingLog!.CaseId)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(existingLog!.CaseId, r.CaseId));
    }

    [SkippableFact]
    public async Task ActivityLog_CanQueryByTimestampRange()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var existingLog = await _fixture.DbContext!.ActivityLogs.FirstOrDefaultAsync();
        Skip.If(existingLog == null, "No ActivityLog records exist");

        var fromDate = existingLog!.Timestamp.AddDays(-1);
        var toDate = existingLog.Timestamp.AddDays(1);

        // Act
        var results = await _fixture.DbContext.ActivityLogs
            .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.True(r.Timestamp >= fromDate);
            Assert.True(r.Timestamp <= toDate);
        });
    }

    [SkippableFact]
    public async Task ActivityLog_CanOrderByTimestampDescending()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Act
        var results = await _fixture.DbContext!.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToListAsync();

        // Assert
        Skip.If(results.Count < 2, "Need at least 2 records to verify ordering");

        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].Timestamp >= results[i + 1].Timestamp,
                "Results should be ordered by Timestamp descending");
        }
    }

    [SkippableFact]
    public async Task ActivityLog_CanPaginateResults()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var totalCount = await _fixture.DbContext!.ActivityLogs.CountAsync();
        Skip.If(totalCount < 2, "Need at least 2 records to test pagination");

        const int pageSize = 5;

        // Act - get first page
        var firstPage = await _fixture.DbContext.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Skip(0)
            .Take(pageSize)
            .ToListAsync();

        // Act - get second page (if exists)
        var secondPage = await _fixture.DbContext.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Skip(pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(firstPage);
        Assert.True(firstPage.Count <= pageSize);

        if (totalCount > pageSize)
        {
            Assert.NotEmpty(secondPage);
            var firstPageIds = firstPage.Select(l => l.Id).ToHashSet();
            Assert.All(secondPage, log => Assert.DoesNotContain(log.Id, firstPageIds));
        }
    }

    [SkippableFact]
    public async Task ActivityLog_HasExpectedAuditFields()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Arrange
        var log = await _fixture.DbContext!.ActivityLogs.FirstOrDefaultAsync();
        Skip.If(log == null, "No ActivityLog records exist");

        // Assert - verify audit fields structure
        Assert.NotEqual(Guid.Empty, log!.Id);
        Assert.NotEqual(default, log.Timestamp);
        Assert.NotEqual(default, log.CreatedAt);
    }

    [SkippableFact]
    public async Task Database_CanConnect()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Act
        var canConnect = await _fixture.DbContext!.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect, "Should be able to connect to the database");
    }

    [SkippableFact]
    public async Task Database_HasAppliedMigrations()
    {
        Skip.If(!_fixture.IsDatabaseConfigured, "Database not configured");

        // Act
        var appliedMigrations = await _fixture.DbContext!.Database.GetAppliedMigrationsAsync();

        // Assert
        Assert.NotEmpty(appliedMigrations);
    }
}

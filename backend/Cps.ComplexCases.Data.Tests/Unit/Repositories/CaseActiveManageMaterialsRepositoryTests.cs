using System.Text.Json;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CPS.ComplexCases.Data.Tests.Unit.Repositories;

public class CaseActiveManageMaterialsRepositoryTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly CaseActiveManageMaterialsRepository _repository;

    private const int CaseId = 123;

    public CaseActiveManageMaterialsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestApplicationDbContext(options);
        _repository = new CaseActiveManageMaterialsRepository(_context);
    }

    private async Task SeedOperationAsync(IEnumerable<string> sourcePaths, IEnumerable<string>? destinationPaths = null)
    {
        var operation = new CaseActiveManageMaterialsOperation
        {
            Id = Guid.NewGuid(),
            CaseId = CaseId,
            OperationType = "Copy",
            SourcePaths = JsonSerializer.Serialize(sourcePaths),
            DestinationPaths = destinationPaths != null ? JsonSerializer.Serialize(destinationPaths) : null,
            CreatedAt = DateTime.UtcNow,
        };

        _context.CaseActiveManageMaterialsOperations.Add(operation);
        await _context.SaveChangesAsync();
    }

    // ── CheckConflictAndInsertAsync ───────────────────────────────────

    [Fact]
    public async Task CheckConflictAndInsertAsync_InsertsRow_WhenNoActiveOperations()
    {
        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderA/file.pdf"], ["Cases/123/Dest/"]),
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/Dest/"]);

        Assert.True(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Incoming source vs existing source
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenIncomingSourceOverlapsExistingSource()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderA/file.pdf"], ["Cases/123/DestB/"]),
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Incoming source vs existing destination
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenIncomingSourceOverlapsExistingDestination()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/SharedDest/report.pdf"], ["Cases/123/DestB/"]),
            sourcePaths: ["Cases/123/SharedDest/report.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Incoming destination vs existing source
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenIncomingDestinationOverlapsExistingSource()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderB/other.pdf"], ["Cases/123/FolderA/"]),
            sourcePaths: ["Cases/123/FolderB/other.pdf"],
            destinationPaths: ["Cases/123/FolderA/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Incoming destination vs existing destination (exact match)
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenIncomingDestinationOverlapsExistingDestination()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderB/file2.pdf"], ["Cases/123/SharedDest/"]),
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Incoming destination is a sub-folder of an existing destination (prefix overlap)
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenIncomingDestinationIsSubFolderOfExistingDestination()
    {
        // Existing operation already writing into Cases/123/Dest/
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/Dest/"]);

        // Incoming targets Cases/123/Dest/SubFolder/ — a path nested inside the existing destination
        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderB/file2.pdf"], ["Cases/123/Dest/SubFolder/"]),
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/Dest/SubFolder/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    // Existing destination is a sub-folder of the incoming destination (reverse prefix overlap)
    [Fact]
    public async Task CheckConflictAndInsertAsync_ReturnsFalse_WhenExistingDestinationIsSubFolderOfIncomingDestination()
    {
        // Existing operation already writing into Cases/123/Dest/SubFolder/
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/Dest/SubFolder/"]);

        // Incoming targets the parent Cases/123/Dest/ — which contains the existing destination
        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderB/file2.pdf"], ["Cases/123/Dest/"]),
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/Dest/"]);

        Assert.False(result);
        Assert.Single(await _context.CaseActiveManageMaterialsOperations.ToListAsync());
    }

    [Fact]
    public async Task CheckConflictAndInsertAsync_InsertsRow_WhenNoPathsOverlap()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.CheckConflictAndInsertAsync(
            BuildOperation(["Cases/123/FolderB/file2.pdf"], ["Cases/123/DestB/"]),
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.True(result);
        Assert.Equal(2, (await _context.CaseActiveManageMaterialsOperations.ToListAsync()).Count);
    }

    [Fact]
    public async Task CheckConflictAndInsertAsync_InsertsRow_WhenOperationsBelongToDifferentCase()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var incoming = new CaseActiveManageMaterialsOperation
        {
            Id = Guid.NewGuid(),
            CaseId = 999,
            OperationType = "BatchCopy",
            SourcePaths = JsonSerializer.Serialize(new[] { "Cases/123/FolderA/file1.pdf" }),
            DestinationPaths = JsonSerializer.Serialize(new[] { "Cases/123/SharedDest/" }),
            CreatedAt = DateTime.UtcNow,
        };

        var result = await _repository.CheckConflictAndInsertAsync(
            incoming,
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        Assert.True(result);
        Assert.Equal(2, (await _context.CaseActiveManageMaterialsOperations.ToListAsync()).Count);
    }

    private static CaseActiveManageMaterialsOperation BuildOperation(
        IEnumerable<string> sourcePaths,
        IEnumerable<string>? destinationPaths = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            CaseId = CaseId,
            OperationType = "BatchCopy",
            SourcePaths = JsonSerializer.Serialize(sourcePaths),
            DestinationPaths = destinationPaths != null ? JsonSerializer.Serialize(destinationPaths) : null,
            CreatedAt = DateTime.UtcNow,
        };

    public void Dispose()
    {
        _context.Dispose();
    }
}

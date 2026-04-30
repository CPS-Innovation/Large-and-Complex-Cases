using System.Text.Json;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;

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

    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsFalse_WhenNoActiveOperations()
    {
        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/Dest/"]);

        Assert.False(result);
    }

    // Incoming source vs existing source
    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsTrue_WhenIncomingSourceOverlapsExistingSource()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.True(result);
    }

    // Incoming source vs existing destination
    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsTrue_WhenIncomingSourceOverlapsExistingDestination()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/SharedDest/report.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.True(result);
    }

    // Incoming destination vs existing source
    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsTrue_WhenIncomingDestinationOverlapsExistingSource()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/FolderB/other.pdf"],
            destinationPaths: ["Cases/123/FolderA/"]);

        Assert.True(result);
    }

    // Incoming destination vs existing destination
    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsTrue_WhenIncomingDestinationOverlapsExistingDestination()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        Assert.True(result);
    }

    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsFalse_WhenNoPathsOverlap()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/DestA/"]);

        var result = await _repository.HasConflictingOperationAsync(
            CaseId,
            sourcePaths: ["Cases/123/FolderB/file2.pdf"],
            destinationPaths: ["Cases/123/DestB/"]);

        Assert.False(result);
    }

    [Fact]
    public async Task HasConflictingOperationAsync_ReturnsFalse_WhenOperationsBelongToDifferentCase()
    {
        await SeedOperationAsync(
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        var result = await _repository.HasConflictingOperationAsync(
            caseId: 999,
            sourcePaths: ["Cases/123/FolderA/file1.pdf"],
            destinationPaths: ["Cases/123/SharedDest/"]);

        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

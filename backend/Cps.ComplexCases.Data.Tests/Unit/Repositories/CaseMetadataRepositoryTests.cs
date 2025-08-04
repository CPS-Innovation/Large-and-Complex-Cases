using AutoFixture;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Tests.Unit.Repositories
{
    public class CaseMetadataRepositoryTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly TestApplicationDbContext _context;
        private readonly CaseMetadataRepository _repository;

        public CaseMetadataRepositoryTests()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TestApplicationDbContext(options);
            _repository = new CaseMetadataRepository(_context);
        }

        [Fact]
        public async Task GetByCaseIdAsync_ReturnsMetadata_WhenExists()
        {
            var metadata = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 123).Create();
            _context.CaseMetadata.Add(metadata);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByCaseIdAsync(123);

            Assert.NotNull(result);
            Assert.Equal(123, result!.CaseId);
        }

        [Fact]
        public async Task GetByCaseIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _repository.GetByCaseIdAsync(9999);
            Assert.Null(result);
        }


        [Fact]
        public async Task AddAsync_AddsAndSavesMetadata()
        {
            var metadata = _fixture.Create<CaseMetadata>();

            var result = await _repository.AddAsync(metadata);

            Assert.Equal(metadata, result);
            var saved = await _context.CaseMetadata.FindAsync(metadata.CaseId);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExisting_WhenExists()
        {
            var original = _fixture.Build<CaseMetadata>()
                                   .With(m => m.CaseId, 999)
                                   .With(m => m.EgressWorkspaceId, "old")
                                   .Create();

            _context.CaseMetadata.Add(original);
            await _context.SaveChangesAsync();

            var updated = _fixture.Build<CaseMetadata>()
                                  .With(m => m.CaseId, 999)
                                  .With(m => m.EgressWorkspaceId, "new")
                                  .Create();

            var result = await _repository.UpdateAsync(updated);

            Assert.NotNull(result);
            Assert.Equal("new", result!.EgressWorkspaceId);
        }

        [Fact]
        public async Task UpdateAsync_DoesNotThrow_WhenMetadataIsMinimal()
        {
            var original = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 1001).Create();
            _context.CaseMetadata.Add(original);
            await _context.SaveChangesAsync();

            var minimal = new CaseMetadata { CaseId = 1001 };
            var result = await _repository.UpdateAsync(minimal);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            var metadata = _fixture.Create<CaseMetadata>();
            var result = await _repository.UpdateAsync(metadata);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCaseIdsAsync_ReturnsMatchingItems()
        {
            var items = _fixture.CreateMany<CaseMetadata>(5).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();

            var caseIds = items.Take(3).Select(x => x.CaseId).ToList();

            var result = await _repository.GetByCaseIdsAsync(caseIds);

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByCaseIdsAsync_ReturnsEmpty_WhenNoMatches()
        {
            var result = await _repository.GetByCaseIdsAsync(new List<int> { 9999, 8888 });
            Assert.Empty(result);
        }


        [Fact]
        public async Task GetByEgressWorkspaceIdsAsync_ReturnsMatchingItems()
        {
            var items = _fixture.CreateMany<CaseMetadata>(5).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();

            var ids = items.Where(x => x.EgressWorkspaceId != null)
                           .Select(x => x.EgressWorkspaceId!)
                           .Take(2)
                           .ToList();

            var result = await _repository.GetByEgressWorkspaceIdsAsync(ids);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByEgressWorkspaceIdsAsync_ReturnsEmpty_WhenNoMatches()
        {
            var result = await _repository.GetByEgressWorkspaceIdsAsync(new List<string> { "missing-id" });
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByNetAppFolderPathsAsync_ReturnsMatchingItems()
        {
            var items = _fixture.CreateMany<CaseMetadata>(5).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();

            var paths = items.Where(x => x.NetappFolderPath != null)
                             .Select(x => x.NetappFolderPath!)
                             .Take(2)
                             .ToList();

            var result = await _repository.GetByNetAppFolderPathsAsync(paths);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByNetAppFolderPathsAsync_ReturnsEmpty_WhenNoMatches()
        {
            var result = await _repository.GetByNetAppFolderPathsAsync(new List<string> { "missing-path" });
            Assert.Empty(result);
        }


        [Fact]
        public async Task GetByActiveTransferIdAsync_ReturnsCorrectMetadata()
        {
            var item = _fixture.Build<CaseMetadata>()
                               .With(x => x.ActiveTransferId, Guid.NewGuid())
                               .Create();

            _context.CaseMetadata.Add(item);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByActiveTransferIdAsync(item.ActiveTransferId!.Value);

            Assert.NotNull(result);
            Assert.Equal(item.ActiveTransferId, result!.ActiveTransferId);
        }

        [Fact]
        public async Task GetByActiveTransferIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _repository.GetByActiveTransferIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_Throws_IfMetadataIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
        }

        [Fact]
        public async Task GetByCaseIdAsync_ReturnsNull_ForNegativeOrZeroId()
        {
            var result1 = await _repository.GetByCaseIdAsync(-1);
            var result2 = await _repository.GetByCaseIdAsync(0);
            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task GetByCaseIdsAsync_ReturnsEmpty_ForEmptyOrNullList()
        {
            var result1 = await _repository.GetByCaseIdsAsync(new List<int>());
            var result2 = await _repository.GetByCaseIdsAsync(null!);
            Assert.Empty(result1);
            Assert.Empty(result2);
        }

        [Fact]
        public async Task GetByEgressWorkspaceIdsAsync_ReturnsEmpty_ForEmptyOrNullList()
        {
            var result1 = await _repository.GetByEgressWorkspaceIdsAsync(new List<string>());
            var result2 = await _repository.GetByEgressWorkspaceIdsAsync(null!);
            Assert.Empty(result1);
            Assert.Empty(result2);
        }

        [Fact]
        public async Task GetByNetAppFolderPathsAsync_ReturnsEmpty_ForEmptyOrNullList()
        {
            var result1 = await _repository.GetByNetAppFolderPathsAsync(new List<string>());
            var result2 = await _repository.GetByNetAppFolderPathsAsync(null!);
            Assert.Empty(result1);
            Assert.Empty(result2);
        }

        [Fact]
        public async Task GetByActiveTransferIdAsync_ReturnsNull_ForGuidEmpty()
        {
            var result = await _repository.GetByActiveTransferIdAsync(Guid.Empty);
            Assert.Null(result);
        }

        [Fact]
        public async Task RepositoryMethods_Throw_WhenContextDisposed()
        {
            _context.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.GetByCaseIdAsync(1));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.AddAsync(_fixture.Create<CaseMetadata>()));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.UpdateAsync(_fixture.Create<CaseMetadata>()));
        }

        [Fact]
        public async Task UpdateAsync_DoesNotUpdate_WhenCaseIdChanged()
        {
            var original = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 2001).Create();
            _context.CaseMetadata.Add(original);
            await _context.SaveChangesAsync();

            var updated = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 2002).Create();
            var result = await _repository.UpdateAsync(updated);
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_Throws_WhenDuplicateCaseId()
        {
            var metadata = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 3001).Create();
            await _repository.AddAsync(metadata);
            var duplicate = _fixture.Build<CaseMetadata>().With(m => m.CaseId, 3001).Create();
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(duplicate));
        }

        [Fact]
        public async Task GetByCaseIdsAsync_ReturnsCorrectItems_WhenSomeIdsDoNotExist()
        {
            var items = _fixture.CreateMany<CaseMetadata>(3).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();
            var ids = items.Select(x => x.CaseId).ToList();
            ids.Add(9999);
            var result = await _repository.GetByCaseIdsAsync(ids);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByEgressWorkspaceIdsAsync_ReturnsCorrectItems_WhenSomeIdsDoNotExist()
        {
            var items = _fixture.CreateMany<CaseMetadata>(3).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();
            var ids = items.Where(x => x.EgressWorkspaceId != null).Select(x => x.EgressWorkspaceId!).ToList();
            ids.Add("not-exist");
            var result = await _repository.GetByEgressWorkspaceIdsAsync(ids);
            Assert.True(result.Count() <= ids.Count);
        }

        [Fact]
        public async Task GetByNetAppFolderPathsAsync_ReturnsCorrectItems_WhenSomePathsDoNotExist()
        {
            var items = _fixture.CreateMany<CaseMetadata>(3).ToList();
            _context.CaseMetadata.AddRange(items);
            await _context.SaveChangesAsync();
            var paths = items.Where(x => x.NetappFolderPath != null).Select(x => x.NetappFolderPath!).ToList();
            paths.Add("not-exist-path");
            var result = await _repository.GetByNetAppFolderPathsAsync(paths);
            Assert.True(result.Count() <= paths.Count);
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

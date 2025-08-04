using AutoFixture;
using AutoFixture.Dsl;
using CPS.ComplexCases.Data.Dtos;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CPS.ComplexCases.Data.Tests.Unit.Repositories
{
    public class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ActivityLog>()
                .Ignore(e => e.Details);
        }
    }

    public class ActivityLogRepositoryTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly TestApplicationDbContext _context;
        private readonly ActivityLogRepository _repository;

        public ActivityLogRepositoryTests()
        {
            _fixture = new Fixture();

            _fixture.Customize<ActivityLog>(composer => composer
                .Without(x => x.Details)
                .With(x => x.Timestamp, () => DateTime.UtcNow)
                .With(x => x.CreatedAt, () => DateTime.UtcNow)
                .With(x => x.UpdatedAt, () => DateTime.UtcNow));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new TestApplicationDbContext(options);
            _repository = new ActivityLogRepository(_context);
        }

        private IPostprocessComposer<ActivityLog> CreateActivityLogBuilder()
        {
            return _fixture.Build<ActivityLog>()
                .Without(x => x.Details)
                .With(x => x.Timestamp, () => DateTime.UtcNow)
                .With(x => x.CreatedAt, () => DateTime.UtcNow)
                .With(x => x.UpdatedAt, () => DateTime.UtcNow);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnActivityLog_WhenFound()
        {
            var activityLog = _fixture.Create<ActivityLog>();
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(activityLog.Id);

            Assert.NotNull(result);
            Assert.Equal(activityLog.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByResourceIdAsync_ShouldReturnFilteredLogs()
        {
            var resourceId = "res123";
            var matchingLogs = CreateActivityLogBuilder()
                .With(a => a.ResourceId, resourceId)
                .CreateMany(3);
            var nonMatchingLogs = CreateActivityLogBuilder()
                .With(a => a.ResourceId, "other")
                .CreateMany(2);

            _context.ActivityLogs.AddRange(matchingLogs);
            _context.ActivityLogs.AddRange(nonMatchingLogs);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByResourceIdAsync(resourceId);

            Assert.Equal(3, result.Count());
            Assert.All(result, log => Assert.Equal(resourceId, log.ResourceId));
        }

        [Fact]
        public async Task AddAsync_ShouldAddAndReturnActivityLog()
        {
            var log = _fixture.Create<ActivityLog>();

            var result = await _repository.AddAsync(log);

            Assert.Equal(log, result);

            var savedLog = await _context.ActivityLogs.FindAsync(log.Id);
            Assert.NotNull(savedLog);
            Assert.Equal(log.Id, savedLog.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedLog_WhenExists()
        {
            // Arrange
            var existingLog = _fixture.Create<ActivityLog>();
            _context.ActivityLogs.Add(existingLog);
            await _context.SaveChangesAsync();

            _context.Entry(existingLog).State = EntityState.Detached;

            var updatedLog = CreateActivityLogBuilder()
                .With(x => x.ActionType, "UpdatedAction")
                .Create();

            var idProperty = typeof(ActivityLog).GetProperty("Id");
            idProperty!.SetValue(updatedLog, existingLog.Id);


            // Act
            var result = await _repository.UpdateAsync(updatedLog);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingLog.Id, result.Id);

            var savedLog = await _context.ActivityLogs.FindAsync(existingLog.Id);
            Assert.Equal("UpdatedAction", savedLog?.ActionType);
        }


        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var log = _fixture.Create<ActivityLog>();

            var result = await _repository.UpdateAsync(log);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldReturnFilteredResults()
        {
            var now = DateTime.UtcNow;
            var logs = CreateActivityLogBuilder()
                .With(x => x.Timestamp, now)
                .With(x => x.UserName, "user1")
                .With(x => x.ActionType, "type1")
                .With(x => x.ResourceType, "resType")
                .With(x => x.ResourceId, "resId")
                .CreateMany(10);

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto
            {
                FromDate = now.AddMinutes(-1),
                ToDate = now.AddMinutes(1),
                Username = "user1",
                ActionType = "type1",
                ResourceType = "resType",
                ResourceId = "resId",
                Skip = 0,
                Take = 5
            };

            var result = await _repository.GetByFilterAsync(filter);

            Assert.NotNull(result);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(5, result.Logs.Count());
            Assert.Equal(0, result.Skip);
            Assert.Equal(5, result.Take);
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldHandlePagination()
        {
            var now = DateTime.UtcNow;
            var logs = CreateActivityLogBuilder()
                .With(x => x.Timestamp, now)
                .With(x => x.UserName, "user1")
                .CreateMany(15);

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto
            {
                Username = "user1",
                Skip = 10,
                Take = 5
            };

            var result = await _repository.GetByFilterAsync(filter);

            Assert.NotNull(result);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Logs.Count());
            Assert.Equal(10, result.Skip);
            Assert.Equal(5, result.Take);
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldFilterByFromDateOnly()
        {
            var now = DateTime.UtcNow;

            var logs = CreateActivityLogBuilder()
                .With(x => x.Timestamp, now.AddMinutes(-10))
                .CreateMany(5)
                .Concat(CreateActivityLogBuilder()
                    .With(x => x.Timestamp, now.AddMinutes(10))
                    .CreateMany(5))
                .ToList();

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto
            {
                FromDate = now,
                Skip = 0,
                Take = 10
            };

            var result = await _repository.GetByFilterAsync(filter);

            Assert.All(result.Logs, log => Assert.True(log.Timestamp >= now));
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldFilterByToDateOnly()
        {
            var now = DateTime.UtcNow;

            var logs = CreateActivityLogBuilder()
                .With(x => x.Timestamp, now.AddMinutes(-10))
                .CreateMany(5)
                .Concat(CreateActivityLogBuilder()
                    .With(x => x.Timestamp, now.AddMinutes(10))
                    .CreateMany(5))
                .ToList();

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto
            {
                ToDate = now,
                Skip = 0,
                Take = 10
            };

            var result = await _repository.GetByFilterAsync(filter);

            Assert.All(result.Logs, log => Assert.True(log.Timestamp <= now));
        }
        [Fact]
        public async Task GetByFilterAsync_ShouldFilterByActionType()
        {
            var logs = CreateActivityLogBuilder()
                .With(x => x.ActionType, "match")
                .CreateMany(3)
                .Concat(CreateActivityLogBuilder()
                    .With(x => x.ActionType, "other")
                    .CreateMany(2))
                .ToList();

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto
            {
                ActionType = "match",
                Skip = 0,
                Take = 10
            };

            var result = await _repository.GetByFilterAsync(filter);

            Assert.All(result.Logs, log => Assert.Equal("match", log.ActionType));
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldReturnOrderedResults()
        {
            var now = DateTime.UtcNow;

            var logs = new List<ActivityLog>
            {
                CreateActivityLogBuilder().With(x => x.Timestamp, now.AddMinutes(1)).Create(),
                CreateActivityLogBuilder().With(x => x.Timestamp, now.AddMinutes(2)).Create(),
                CreateActivityLogBuilder().With(x => x.Timestamp, now.AddMinutes(3)).Create(),
            };

            _context.ActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            var filter = new ActivityLogFilterDto { Skip = 0, Take = 10 };
            var result = await _repository.GetByFilterAsync(filter);

            var orderedTimestamps = result.Logs.Select(l => l.Timestamp).ToList();
            var expectedOrder = orderedTimestamps.OrderByDescending(x => x).ToList();

            Assert.Equal(expectedOrder, orderedTimestamps);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

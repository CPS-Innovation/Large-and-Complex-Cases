using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class PurgeTransferHistoryTests
{
    private readonly Mock<ILogger<PurgeTransferHistory>> _mockLogger;
    private readonly DurableTaskClientStub _clientStub;
    private readonly DurableEntityClientStub _entityClientStub;
    private readonly PurgeRetentionConfig _config;
    private readonly PurgeTransferHistory _function;

    public PurgeTransferHistoryTests()
    {
        _mockLogger = new Mock<ILogger<PurgeTransferHistory>>();
        _entityClientStub = new DurableEntityClientStub("test-entity-client");
        _clientStub = new DurableTaskClientStub(_entityClientStub);
        
        _config = new PurgeRetentionConfig { RetentionDays = 30 };
        var options = Options.Create(_config);
        
        _function = new PurgeTransferHistory(_mockLogger.Object, options);
    }

    [Fact]
    public async Task Run_FiltersInstancesByRetentionCutoff_UsesCorrectCutoffDate()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var cancellationToken = CancellationToken.None;
        
        var mockAsyncPageable = CreateAsyncPageable(Array.Empty<OrchestrationMetadata>());
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(0));

        // Act
        await _function.Run(timerInfo, _clientStub, cancellationToken);

        // Assert
        var capturedQuery = _clientStub.CapturedQuery;
        Assert.NotNull(capturedQuery);
        Assert.NotNull(capturedQuery.CreatedTo);
        
        var expectedCutoff = DateTimeOffset.UtcNow.AddDays(-_config.RetentionDays);
        var cutoffDifference = Math.Abs((capturedQuery.CreatedTo.Value - expectedCutoff).TotalSeconds);
        Assert.True(cutoffDifference < 5, "Cutoff date should be within 5 seconds of expected value");
        
        Assert.NotNull(capturedQuery.Statuses);
        Assert.Equal(3, capturedQuery.Statuses.Count());
        Assert.Contains(OrchestrationRuntimeStatus.Completed, capturedQuery.Statuses);
        Assert.Contains(OrchestrationRuntimeStatus.Failed, capturedQuery.Statuses);
        Assert.Contains(OrchestrationRuntimeStatus.Terminated, capturedQuery.Statuses);
    }

    [Fact]
    public async Task Run_WithMultipleInstances_SignalsDeleteForEachEntity()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var instances = new[]
        {
            CreateOrchestrationMetadata("instance-1"),
            CreateOrchestrationMetadata("instance-2"),
            CreateOrchestrationMetadata("instance-3")
        };
        
        var mockAsyncPageable = CreateAsyncPageable(instances);
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(3));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        Assert.Equal(3, _entityClientStub.SignalledEntityIds.Count);
        Assert.All(_entityClientStub.SignalledEntityIds, id => Assert.Equal(nameof(TransferEntityState), id.Name, StringComparer.OrdinalIgnoreCase));
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-1");
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-2");
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-3");
    }

    [Fact]
    public async Task Run_WhenEntitySignalFails_ContinuesProcessingRemainingEntities()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var instances = new[]
        {
            CreateOrchestrationMetadata("instance-1"),
            CreateOrchestrationMetadata("instance-2"),
            CreateOrchestrationMetadata("instance-3")
        };
        
        var mockAsyncPageable = CreateAsyncPageable(instances);
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(3));

        _entityClientStub.OnSignalEntityAsync = (entityId, operation, input, options, ct) =>
        {
            // Fail on second instance
            if (entityId.Key == "instance-2")
                throw new InvalidOperationException("Entity signal failed");
            return Task.CompletedTask;
        };

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        Assert.Equal(3, _entityClientStub.SignalledEntityIds.Count);
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-1");
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-2");
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-3");
        
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("instance-2")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenPurgeAllInstancesFails_LogsWarningAndCompletes()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var instances = new[]
        {
            CreateOrchestrationMetadata("instance-1")
        };
        
        var mockAsyncPageable = CreateAsyncPageable(instances);
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;

        var purgeException = new InvalidOperationException("Purge operation failed");
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => throw purgeException;

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        Assert.Single(_entityClientStub.SignalledEntityIds);
        Assert.Equal("Delete", _entityClientStub.SignalledCalls[0].Operation);

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to purge orchestration instances")),
                It.Is<Exception>(ex => ex == purgeException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_SkipsInstancesWithNullOrEmptyInstanceId()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var instances = new[]
        {
            CreateOrchestrationMetadata("instance-1"),
            CreateOrchestrationMetadata(""),
            CreateOrchestrationMetadata(null!),
            CreateOrchestrationMetadata("instance-2")
        };
        
        var mockAsyncPageable = CreateAsyncPageable(instances);
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(2));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        Assert.Equal(2, _entityClientStub.SignalledEntityIds.Count);
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-1");
        Assert.Contains(_entityClientStub.SignalledEntityIds, id => id.Key == "instance-2");
    }

    [Fact]
    public async Task Run_PassesCancellationTokenThroughout()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        var mockAsyncPageable = CreateAsyncPageable(Array.Empty<OrchestrationMetadata>());
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;

        CancellationToken? capturedPurgeToken = null;
        _clientStub.OnPurgeAllInstancesAsync = (filter, ct) =>
        {
            capturedPurgeToken = ct;
            return Task.FromResult(new PurgeResult(0));
        };

        // Act
        await _function.Run(timerInfo, _clientStub, cancellationToken);

        // Assert
        Assert.NotNull(capturedPurgeToken);
        Assert.Equal(cancellationToken, capturedPurgeToken.Value);
    }

    [Fact]
    public async Task Run_LogsStartInformationWithRetentionDetails()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var mockAsyncPageable = CreateAsyncPageable(Array.Empty<OrchestrationMetadata>());
        
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(0));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("PurgeTransferHistory started") &&
                    v.ToString()!.Contains("30 days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_LogsEntityDeleteCount()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var instances = new[]
        {
            CreateOrchestrationMetadata("instance-1"),
            CreateOrchestrationMetadata("instance-2")
        };
        
        var mockAsyncPageable = CreateAsyncPageable(instances);
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(2));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Signalled delete for 2 transfer entities")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_LogsPurgeResultCount()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var mockAsyncPageable = CreateAsyncPageable(Array.Empty<OrchestrationMetadata>());
        
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(42));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Purged 42 orchestration instances")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_PurgeFilterUsesCorrectParameters()
    {
        // Arrange
        var timerInfo = CreateTimerInfo();
        var mockAsyncPageable = CreateAsyncPageable(Array.Empty<OrchestrationMetadata>());
        
        _clientStub.OnGetAllInstancesAsync = _ => mockAsyncPageable;
        _clientStub.OnPurgeAllInstancesAsync = (_, _) => Task.FromResult(new PurgeResult(0));

        // Act
        await _function.Run(timerInfo, _clientStub);

        // Assert
        var capturedFilter = _clientStub.CapturedPurgeFilter;
        Assert.NotNull(capturedFilter);
        Assert.NotNull(capturedFilter.CreatedTo);
        
        var expectedCutoff = DateTimeOffset.UtcNow.AddDays(-_config.RetentionDays);
        var cutoffDifference = Math.Abs((capturedFilter.CreatedTo.Value - expectedCutoff).TotalSeconds);
        Assert.True(cutoffDifference < 5, "Purge filter cutoff should match query cutoff");
        
        Assert.NotNull(capturedFilter.Statuses);
        Assert.Equal(3, capturedFilter.Statuses.Count());
        Assert.Contains(OrchestrationRuntimeStatus.Completed, capturedFilter.Statuses);
        Assert.Contains(OrchestrationRuntimeStatus.Failed, capturedFilter.Statuses);
        Assert.Contains(OrchestrationRuntimeStatus.Terminated, capturedFilter.Statuses);
    }

    private static TimerInfo CreateTimerInfo()
    {
        return new TimerInfo
        {
            ScheduleStatus = new ScheduleStatus(),
            IsPastDue = false
        };
    }

    private static OrchestrationMetadata CreateOrchestrationMetadata(string instanceId)
    {
        return new OrchestrationMetadata(
            new TaskName("TestOrchestration"),
            instanceId);
    }

    private static AsyncPageable<OrchestrationMetadata> CreateAsyncPageable(
        IEnumerable<OrchestrationMetadata> items)
    {
        var itemList = items.ToList();
        return Pageable.Create((continuationToken, pageSizeHint, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(continuationToken))
            {
                return Task.FromResult(new Page<OrchestrationMetadata>(Array.Empty<OrchestrationMetadata>(), null!));
            }

            return Task.FromResult(new Page<OrchestrationMetadata>(itemList, null!));
        });
    }
}
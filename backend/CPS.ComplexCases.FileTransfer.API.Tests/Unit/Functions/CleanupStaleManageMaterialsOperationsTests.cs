using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoFixture;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class CleanupStaleManageMaterialsOperationsTests
{
    private readonly Mock<ILogger<CleanupStaleManageMaterialsOperations>> _loggerMock;
    private readonly Mock<ICaseActiveManageMaterialsService> _serviceMock;
    private readonly DurableEntityClientStub _entityClientStub;
    private readonly ManageMaterialsCleanupConfig _config;
    private readonly CleanupStaleManageMaterialsOperations _function;
    private readonly Fixture _fixture;

    public CleanupStaleManageMaterialsOperationsTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<CleanupStaleManageMaterialsOperations>>();
        _serviceMock = new Mock<ICaseActiveManageMaterialsService>();
        _entityClientStub = new DurableEntityClientStub("TestClient");

        _config = new ManageMaterialsCleanupConfig { MaxAgeHours = 24 };

        _serviceMock
            .Setup(s => s.DeleteOperationAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _function = new CleanupStaleManageMaterialsOperations(
            _loggerMock.Object,
            _serviceMock.Object,
            Options.Create(_config));
    }

    // ── Age-based deletion ────────────────────────────────────────────

    [Fact]
    public async Task Run_WhenRowExceedsMaxAge_DeletesWithoutCheckingDurableStatus()
    {
        var expiredId = Guid.NewGuid();
        var expiredRow = MakeRow(expiredId, createdAt: DateTime.UtcNow.AddHours(-(_config.MaxAgeHours + 1)));
        SetupService([expiredRow]);
        var client = CreateClient(instanceIdToStatus: new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(expiredId), Times.Once);
        client.Verify(c => c.GetInstancesAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenRowIsExactlyAtMaxAge_DeletesWithoutCheckingDurableStatus()
    {
        var id = Guid.NewGuid();
        var row = MakeRow(id, createdAt: DateTime.UtcNow.AddHours(-_config.MaxAgeHours).AddSeconds(-1));
        SetupService([row]);
        var client = CreateClient(instanceIdToStatus: new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(id), Times.Once);
    }

    // ── Status-based deletion ─────────────────────────────────────────

    [Theory]
    [InlineData(OrchestrationRuntimeStatus.Completed)]
    [InlineData(OrchestrationRuntimeStatus.Failed)]
    [InlineData(OrchestrationRuntimeStatus.Terminated)]
    public async Task Run_WhenOrchestrationIsTerminal_DeletesRow(OrchestrationRuntimeStatus terminalStatus)
    {
        var id = Guid.NewGuid();
        var row = MakeRow(id, createdAt: DateTime.UtcNow.AddMinutes(-5));
        SetupService([row]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>
        {
            { id.ToString(), terminalStatus }
        });

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(id), Times.Once);
    }

    [Fact]
    public async Task Run_WhenOrchestrationInstanceNotFound_DeletesRow()
    {
        var id = Guid.NewGuid();
        var row = MakeRow(id, createdAt: DateTime.UtcNow.AddMinutes(-5));
        SetupService([row]);

        var client = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        client.Setup(c => c.GetInstancesAsync(id.ToString(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null);

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(id), Times.Once);
    }

    [Fact]
    public async Task Run_WhenOrchestrationIsInProgress_SkipsRow()
    {
        var id = Guid.NewGuid();
        var row = MakeRow(id, createdAt: DateTime.UtcNow.AddMinutes(-5));
        SetupService([row]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>
        {
            { id.ToString(), OrchestrationRuntimeStatus.Running }
        });

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenOrchestrationIsPending_SkipsRow()
    {
        var id = Guid.NewGuid();
        var row = MakeRow(id, createdAt: DateTime.UtcNow.AddMinutes(-5));
        SetupService([row]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>
        {
            { id.ToString(), OrchestrationRuntimeStatus.Pending }
        });

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(It.IsAny<Guid>()), Times.Never);
    }

    // ── Mixed batch ───────────────────────────────────────────────────

    [Fact]
    public async Task Run_WithMixedRows_DeletesOnlyEligibleOnes()
    {
        var expiredId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        var runningId = Guid.NewGuid();

        SetupService(
        [
            MakeRow(expiredId, createdAt: DateTime.UtcNow.AddHours(-48)),
            MakeRow(completedId, createdAt: DateTime.UtcNow.AddMinutes(-10)),
            MakeRow(runningId, createdAt: DateTime.UtcNow.AddMinutes(-10)),
        ]);

        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>
        {
            { completedId.ToString(), OrchestrationRuntimeStatus.Completed },
            { runningId.ToString(), OrchestrationRuntimeStatus.Running },
        });

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(expiredId), Times.Once);
        _serviceMock.Verify(s => s.DeleteOperationAsync(completedId), Times.Once);
        _serviceMock.Verify(s => s.DeleteOperationAsync(runningId), Times.Never);
        _serviceMock.Verify(s => s.DeleteOperationAsync(It.IsAny<Guid>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Run_WithNoRows_CompletesWithoutError()
    {
        SetupService([]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(It.IsAny<Guid>()), Times.Never);
    }

    // ── Error resilience ──────────────────────────────────────────────

    [Fact]
    public async Task Run_WhenDeleteFailsForOneRow_ContinuesProcessingRemainingRows()
    {
        var failingId = Guid.NewGuid();
        var successId = Guid.NewGuid();

        SetupService(
        [
            MakeRow(failingId, createdAt: DateTime.UtcNow.AddHours(-48)),
            MakeRow(successId, createdAt: DateTime.UtcNow.AddHours(-48)),
        ]);

        _serviceMock
            .Setup(s => s.DeleteOperationAsync(failingId))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(successId), Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteFailsForOneRow_LogsWarning()
    {
        var failingId = Guid.NewGuid();
        SetupService([MakeRow(failingId, createdAt: DateTime.UtcNow.AddHours(-48))]);

        _serviceMock
            .Setup(s => s.DeleteOperationAsync(failingId))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        // The "Skipping" warning carries the exception as a structured parameter
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(failingId.ToString())),
            It.Is<Exception>(ex => ex is InvalidOperationException),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenGetInstancesAsyncFails_ContinuesProcessingRemainingRows()
    {
        var failingId = Guid.NewGuid();
        var successId = Guid.NewGuid();

        SetupService(
        [
            MakeRow(failingId, createdAt: DateTime.UtcNow.AddMinutes(-5)),
            MakeRow(successId, createdAt: DateTime.UtcNow.AddMinutes(-5)),
        ]);

        var client = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };
        client.Setup(c => c.GetInstancesAsync(failingId.ToString(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Durable client error"));
        client.Setup(c => c.GetInstancesAsync(successId.ToString(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCompletedInstance(successId.ToString()));

        await _function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(successId), Times.Once);
        _serviceMock.Verify(s => s.DeleteOperationAsync(failingId), Times.Never);
    }

    // ── Logging ───────────────────────────────────────────────────────

    [Fact]
    public async Task Run_LogsStartMessage()
    {
        SetupService([]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) =>
                v.ToString()!.Contains("CleanupStaleManageMaterialsOperations started") &&
                v.ToString()!.Contains(_config.MaxAgeHours.ToString())),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_LogsCompletionWithDeletedCount()
    {
        var id = Guid.NewGuid();
        SetupService([MakeRow(id, createdAt: DateTime.UtcNow.AddHours(-48))]);
        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>());

        await _function.Run(CreateTimerInfo(), client.Object);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) =>
                v.ToString()!.Contains("completed") &&
                v.ToString()!.Contains("1")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Config ────────────────────────────────────────────────────────

    [Fact]
    public async Task Run_RespectsConfiguredMaxAgeHours()
    {
        var customConfig = new ManageMaterialsCleanupConfig { MaxAgeHours = 2 };
        var function = new CleanupStaleManageMaterialsOperations(
            _loggerMock.Object,
            _serviceMock.Object,
            Options.Create(customConfig));

        var recentId = Guid.NewGuid();
        var expiredId = Guid.NewGuid();

        SetupService(
        [
            MakeRow(recentId, createdAt: DateTime.UtcNow.AddHours(-1)),   // within 2h window → not expired
            MakeRow(expiredId, createdAt: DateTime.UtcNow.AddHours(-3)),  // outside 2h window → expired
        ]);

        var client = CreateClient(new Dictionary<string, OrchestrationRuntimeStatus>
        {
            { recentId.ToString(), OrchestrationRuntimeStatus.Running }
        });

        await function.Run(CreateTimerInfo(), client.Object);

        _serviceMock.Verify(s => s.DeleteOperationAsync(expiredId), Times.Once);
        _serviceMock.Verify(s => s.DeleteOperationAsync(recentId), Times.Never);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private void SetupService(IEnumerable<CaseActiveManageMaterialsOperation> rows)
    {
        _serviceMock
            .Setup(s => s.GetAllActiveOperationsAsync())
            .ReturnsAsync(rows);
    }

    private Mock<DurableTaskClientStub> CreateClient(
        Dictionary<string, OrchestrationRuntimeStatus> instanceIdToStatus)
    {
        var mock = new Mock<DurableTaskClientStub>(_entityClientStub) { CallBase = true };

        mock.Setup(c => c.GetInstancesAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns((string id, bool _, CancellationToken __) =>
            {
                if (instanceIdToStatus.TryGetValue(id, out var status))
                {
                    var metadata = new OrchestrationMetadata(new TaskName("CopyOrchestrator"), id)
                    {
                        RuntimeStatus = status
                    };
                    return Task.FromResult<OrchestrationMetadata?>(metadata);
                }
                return Task.FromResult<OrchestrationMetadata?>(null);
            });

        return mock;
    }

    private static OrchestrationMetadata MakeCompletedInstance(string instanceId) =>
        new(new TaskName("CopyOrchestrator"), instanceId)
        {
            RuntimeStatus = OrchestrationRuntimeStatus.Completed
        };

    private static CaseActiveManageMaterialsOperation MakeRow(Guid id, DateTime createdAt) =>
        new()
        {
            Id = id,
            CaseId = 42,
            OperationType = "BatchCopy",
            SourcePaths = "[\"CaseRoot/file.txt\"]",
            CreatedAt = createdAt,
        };

    private static TimerInfo CreateTimerInfo() =>
        new() { ScheduleStatus = new ScheduleStatus(), IsPastDue = false };
}

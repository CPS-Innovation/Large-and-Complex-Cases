using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Orchestration;

public class TransferOrchestratorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<TaskOrchestrationContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<SizeConfig>> _sizeConfigMock;
    private readonly TransferOrchestrator _orchestrator;

    public TransferOrchestratorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _activityLogServiceMock = new Mock<IActivityLogService>();
        _contextMock = new Mock<TaskOrchestrationContext>();
        _loggerMock = new Mock<ILogger>();
        _sizeConfigMock = new Mock<IOptions<SizeConfig>>();

        // Provide a default SizeConfig for tests
        _sizeConfigMock.Setup(x => x.Value).Returns(new SizeConfig { BatchSize = 10 });

        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _orchestrator = new TransferOrchestrator(_activityLogServiceMock.Object, _sizeConfigMock.Object);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_ExecutesAllActivitiesInCorrectOrder()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var activityCallOrder = new List<string>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) => activityCallOrder.Add(taskName.Name));

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Equal(6, activityCallOrder.Count);
        Assert.Equal("InitializeTransfer", activityCallOrder[0]);
        Assert.Equal("UpdateActivityLog", activityCallOrder[1]);
        Assert.Equal("UpdateTransferStatus", activityCallOrder[2]);
        Assert.Equal("TransferFile", activityCallOrder[3]);
        Assert.Equal("UpdateActivityLog", activityCallOrder[4]);
        Assert.Equal("FinalizeTransfer", activityCallOrder[5]);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsInitializeTransferWithCorrectEntity()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        TransferEntity? capturedEntity = null;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, entity, _) =>
            {
                if (taskName.Name == "InitializeTransfer")
                {
                    capturedEntity = (TransferEntity)entity;
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(transferPayload.TransferId, capturedEntity.Id);
        Assert.Equal(TransferStatus.Initiated, capturedEntity.Status);
        Assert.Equal(transferPayload.DestinationPath, capturedEntity.DestinationPath);
        Assert.Equal(transferPayload.SourcePaths, capturedEntity.SourcePaths);
        Assert.Equal(transferPayload.CaseId, capturedEntity.CaseId);
        Assert.Equal(transferPayload.TransferType, capturedEntity.TransferType);
        Assert.Equal(transferPayload.TransferDirection, capturedEntity.Direction);
        Assert.Equal(transferPayload.SourcePaths.Count, capturedEntity.TotalFiles);
        Assert.Equal(transferPayload.IsRetry ?? false, capturedEntity.IsRetry);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsUpdateActivityLogWithCorrectInitiatedPayload()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        UpdateActivityLogPayload? capturedPayload = null;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "UpdateActivityLog" && payload is UpdateActivityLogPayload activityPayload)
                {
                    if (capturedPayload == null)
                    {
                        capturedPayload = activityPayload;
                    }
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.NotNull(capturedPayload);
        Assert.Equal(ActivityLog.Enums.ActionType.TransferInitiated, capturedPayload.ActionType);
        Assert.Equal(transferPayload.TransferId.ToString(), capturedPayload.TransferId);
        Assert.Equal(transferPayload.UserName, capturedPayload.UserName);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsUpdateTransferStatusWithInProgress()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        UpdateTransferStatusPayload? capturedPayload = null;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "UpdateTransferStatus" && payload is UpdateTransferStatusPayload statusPayload)
                {
                    capturedPayload = statusPayload;
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.NotNull(capturedPayload);
        Assert.Equal(transferPayload.TransferId, capturedPayload.TransferId);
        Assert.Equal(TransferStatus.InProgress, capturedPayload.Status);
    }

    [Fact]
    public async Task RunOrchestrator_WithMultipleSourcePaths_CallsTransferFileForEachPath()
    {
        // Arrange
        var transferPayload = CreateTransferPayloadWithMultiplePaths();
        var capturedTransferPayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "TransferFile" && payload is TransferFilePayload transferFilePayload)
                {
                    capturedTransferPayloads.Add(transferFilePayload);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Equal(transferPayload.SourcePaths.Count, capturedTransferPayloads.Count);

        for (int i = 0; i < transferPayload.SourcePaths.Count; i++)
        {
            var captured = capturedTransferPayloads[i];
            var expected = transferPayload.SourcePaths[i];

            Assert.Equal(expected, captured.SourcePath);
            Assert.Equal(transferPayload.DestinationPath, captured.DestinationPath);
            Assert.Equal(transferPayload.TransferId, captured.TransferId);
            Assert.Equal(transferPayload.TransferType, captured.TransferType);
            Assert.Equal(transferPayload.TransferDirection, captured.TransferDirection);
            Assert.Equal(transferPayload.WorkspaceId, captured.WorkspaceId);
        }
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsUpdateActivityLogWithCorrectCompletedPayload()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var capturedPayloads = new List<UpdateActivityLogPayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "UpdateActivityLog" && payload is UpdateActivityLogPayload activityPayload)
                {
                    capturedPayloads.Add(activityPayload);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Equal(2, capturedPayloads.Count);

        var completedPayload = capturedPayloads[1];
        Assert.Equal(ActivityLog.Enums.ActionType.TransferCompleted, completedPayload.ActionType);
        Assert.Equal(transferPayload.TransferId.ToString(), completedPayload.TransferId);
        Assert.Equal(transferPayload.UserName, completedPayload.UserName);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsFinalizeTransferWithCorrectPayload()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        FinalizeTransferPayload? capturedPayload = null;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "FinalizeTransfer" && payload is FinalizeTransferPayload finalizePayload)
                {
                    capturedPayload = finalizePayload;
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.NotNull(capturedPayload);
        Assert.Equal(transferPayload.TransferId, capturedPayload.TransferId);
    }

    [Fact]
    public async Task RunOrchestrator_WhenActivityThrowsException_UpdatesTransferStatusToFailedAndRethrows()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var exception = new InvalidOperationException("Test exception");
        var capturedStatusPayloads = new List<UpdateTransferStatusPayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((taskName, payload, options) =>
            {
                if (taskName.Name == "TransferFile")
                {
                    throw exception;
                }

                if (taskName.Name == "UpdateTransferStatus" && payload is UpdateTransferStatusPayload statusPayload)
                {
                    capturedStatusPayloads.Add(statusPayload);
                }

                return Task.CompletedTask;
            });

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunOrchestrator(_contextMock.Object));

        Assert.Equal(exception, thrownException);
        Assert.Equal(2, capturedStatusPayloads.Count);
        Assert.Equal(TransferStatus.InProgress, capturedStatusPayloads[0].Status);
        Assert.Equal(TransferStatus.Failed, capturedStatusPayloads[1].Status);
        Assert.Equal(transferPayload.TransferId, capturedStatusPayloads[1].TransferId);
    }

    [Fact]
    public async Task RunOrchestrator_WhenActivityThrowsException_CallsActivityLogServiceForFailureAndRethrows()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var exception = new InvalidOperationException("Test exception");

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((taskName, payload, options) =>
            {
                if (taskName.Name == "TransferFile")
                {
                    throw exception;
                }
                return Task.CompletedTask;
            });

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunOrchestrator(_contextMock.Object));

        Assert.Equal(exception, thrownException);

        _activityLogServiceMock.Verify(
            x => x.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.TransferFailed,
                ActivityLog.Enums.ResourceType.FileTransfer,
                transferPayload.CaseId,
                transferPayload.TransferId.ToString(),
                It.IsAny<string>(),
                transferPayload.UserName,
                It.IsAny<JsonDocument>()),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns((TransferPayload?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _orchestrator.RunOrchestrator(_contextMock.Object));
    }

    private TransferPayload CreateValidTransferPayload()
    {
        return new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = _fixture.Create<string>(),
            SourcePaths = new List<TransferSourcePath>
            {
                new TransferSourcePath
                {
                    Path = _fixture.Create<string>(),
                }
            },
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            TransferDirection = _fixture.Create<TransferDirection>(),
            WorkspaceId = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = _fixture.Create<bool>()
        };
    }
    private TransferPayload CreateTransferPayloadWithMultiplePaths()
    {
        return new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = _fixture.Create<string>(),
            SourcePaths = new List<TransferSourcePath>
        {
            new TransferSourcePath
            {
                Path = _fixture.Create<string>(),
            },

            new TransferSourcePath
            {
                Path = _fixture.Create<string>(),
            }
        },
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            TransferDirection = _fixture.Create<TransferDirection>(),
            WorkspaceId = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = _fixture.Create<bool>()
        };
    }

}
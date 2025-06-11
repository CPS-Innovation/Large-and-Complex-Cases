using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Orchestration;

public class TransferOrchestratorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<TaskOrchestrationContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly TransferOrchestrator _orchestrator;

    public TransferOrchestratorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _activityLogServiceMock = new Mock<IActivityLogService>();
        _contextMock = new Mock<TaskOrchestrationContext>();
        _loggerMock = new Mock<ILogger>();

        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _orchestrator = new TransferOrchestrator(_activityLogServiceMock.Object);
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
        using (new AssertionScope())
        {
            activityCallOrder.Should().HaveCount(6);
            activityCallOrder[0].Should().Be("IntializeTransfer");
            activityCallOrder[1].Should().Be("UpdateActivityLog");
            activityCallOrder[2].Should().Be("UpdateTransferStatus");
            activityCallOrder[3].Should().Be("TransferFile");
            activityCallOrder[4].Should().Be("UpdateActivityLog");
            activityCallOrder[5].Should().Be("FinalizeTransfer");
        }
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
                if (taskName.Name == "IntializeTransfer")
                {
                    capturedEntity = (TransferEntity)entity;
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        using (new AssertionScope())
        {
            capturedEntity.Should().NotBeNull();
            capturedEntity.Id.Should().Be(transferPayload.TransferId);
            capturedEntity.Status.Should().Be(TransferStatus.Initiated);
            capturedEntity.DestinationPath.Should().Be(transferPayload.DestinationPath);
            capturedEntity.SourcePaths.Should().BeEquivalentTo(transferPayload.SourcePaths);
            capturedEntity.CaseId.Should().Be(transferPayload.CaseId);
            capturedEntity.TransferType.Should().Be(transferPayload.TransferType);
            capturedEntity.Direction.Should().Be(transferPayload.TransferDirection);
            capturedEntity.TotalFiles.Should().Be(transferPayload.SourcePaths.Count);
            capturedEntity.IsRetry.Should().Be(transferPayload.IsRetry ?? false);
        }
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
        using (new AssertionScope())
        {
            capturedPayload.Should().NotBeNull();
            capturedPayload.ActionType.Should().Be(ActivityLog.Enums.ActionType.TransferInitiated);
            capturedPayload.TransferId.Should().Be(transferPayload.TransferId.ToString());
            capturedPayload.UserName.Should().Be(transferPayload.UserName);
        }
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
        using (new AssertionScope())
        {
            capturedPayload.Should().NotBeNull();
            capturedPayload.TransferId.Should().Be(transferPayload.TransferId);
            capturedPayload.Status.Should().Be(TransferStatus.InProgress);
        }
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
        using (new AssertionScope())
        {
            capturedTransferPayloads.Should().HaveCount(transferPayload.SourcePaths.Count);

            for (int i = 0; i < transferPayload.SourcePaths.Count; i++)
            {
                var captured = capturedTransferPayloads[i];
                var expected = transferPayload.SourcePaths[i];

                captured.SourcePath.Should().Be(expected);
                captured.DestinationPath.Should().Be(transferPayload.DestinationPath);
                captured.TransferId.Should().Be(transferPayload.TransferId);
                captured.TransferType.Should().Be(transferPayload.TransferType);
                captured.TransferDirection.Should().Be(transferPayload.TransferDirection);
                captured.WorkspaceId.Should().Be(transferPayload.WorkspaceId);
            }
        }
    }

    [Fact]
    public async Task RunOrchestrator_WithIgnoreOverwritePolicy_SkipsTransferFileForIgnoredPaths()
    {
        // Arrange
        var transferPayload = CreateTransferPayloadWithIgnoredPaths();
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
        var nonIgnoredPaths = transferPayload.SourcePaths.Count(sp => sp.OverwritePolicy != TransferOverwritePolicy.Ignore);
        capturedTransferPayloads.Should().HaveCount(nonIgnoredPaths);
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
        using (new AssertionScope())
        {
            capturedPayloads.Should().HaveCount(2);

            var completedPayload = capturedPayloads[1];
            completedPayload.ActionType.Should().Be(ActivityLog.Enums.ActionType.TransferCompleted);
            completedPayload.TransferId.Should().Be(transferPayload.TransferId.ToString());
            completedPayload.UserName.Should().Be(transferPayload.UserName);
        }
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
        using (new AssertionScope())
        {
            capturedPayload.Should().NotBeNull();
            capturedPayload.TransferId.Should().Be(transferPayload.TransferId);
        }
    }

    [Fact]
    public async Task RunOrchestrator_WhenActivityThrowsException_UpdatesTransferStatusToFailed()
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

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        using (new AssertionScope())
        {
            capturedStatusPayloads.Should().HaveCount(2);
            capturedStatusPayloads[0].Status.Should().Be(TransferStatus.InProgress);
            capturedStatusPayloads[1].Status.Should().Be(TransferStatus.Failed);
            capturedStatusPayloads[1].TransferId.Should().Be(transferPayload.TransferId);
        }
    }

    [Fact]
    public async Task RunOrchestrator_WhenActivityThrowsException_CallsActivityLogServiceForFailure()
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

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
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
                    OverwritePolicy = TransferOverwritePolicy.Overwrite
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
                OverwritePolicy = TransferOverwritePolicy.Overwrite
            },

            new TransferSourcePath
            {
                Path = _fixture.Create<string>(),
                OverwritePolicy = TransferOverwritePolicy.Overwrite
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

    private TransferPayload CreateTransferPayloadWithIgnoredPaths()
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
                OverwritePolicy = TransferOverwritePolicy.Overwrite
            },
            new TransferSourcePath
            {
                Path = _fixture.Create<string>(),
                OverwritePolicy = TransferOverwritePolicy.Ignore
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
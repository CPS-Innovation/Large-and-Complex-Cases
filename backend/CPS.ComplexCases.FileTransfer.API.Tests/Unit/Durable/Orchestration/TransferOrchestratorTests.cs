using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Orchestration;

public class TransferOrchestratorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<TaskOrchestrationContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<SizeConfig>> _sizeConfigMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly Mock<IInitializationHandler> _initializationHandler;
    private readonly SizeConfig _sizeConfig;
    private readonly TransferOrchestrator _orchestrator;

    public TransferOrchestratorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _contextMock = new Mock<TaskOrchestrationContext>();
        _loggerMock = new Mock<ILogger>();
        _sizeConfigMock = new Mock<IOptions<SizeConfig>>();
        _telemetryClientMock = new Mock<ITelemetryClient>();
        _initializationHandler = new Mock<IInitializationHandler>();

        _sizeConfig = new SizeConfig { BatchSize = 10 };
        _sizeConfigMock.Setup(x => x.Value).Returns(_sizeConfig);

        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _contextMock.Setup(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((_, payload, __) =>
            {
                var sourcePaths = (payload as ValidateSourceFilesPayload)?.SourcePaths ?? [];
                return Task.FromResult(new ValidateSourceFilesResult
                {
                    Available = sourcePaths
                });
            });

        _orchestrator = new TransferOrchestrator(_sizeConfigMock.Object, _telemetryClientMock.Object, _initializationHandler.Object);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_ExecutesAllActivitiesInCorrectOrder()
    {
        // Arrange
        var expectedActivityCount = 6;
        var transferPayload = CreateValidTransferPayload();
        var activityCallOrder = new List<string>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) => activityCallOrder.Add(taskName.Name));

        _contextMock.Setup(c => c.CallActivityAsync<ValidateSourceFilesResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((_, payload, __) =>
            {
                var sourcePaths = (payload as ValidateSourceFilesPayload)?.SourcePaths ?? [];
                return Task.FromResult(new ValidateSourceFilesResult { Available = sourcePaths });
            })
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) => activityCallOrder.Add(taskName.Name));

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }))
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) => activityCallOrder.Add(taskName.Name));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Equal(expectedActivityCount, activityCallOrder.Count);
        Assert.Equal("UpdateActivityLog", activityCallOrder[0]);
        Assert.Equal("ValidateSourceFiles", activityCallOrder[1]);
        Assert.Equal("UpdateTransferStatus", activityCallOrder[2]);
        Assert.Equal("TransferFile", activityCallOrder[3]);
        Assert.Equal("FinalizeTransfer", activityCallOrder[4]);
        Assert.Equal("UpdateActivityLog", activityCallOrder[5]);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_UpdatesEntityWithCorrectData()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        TransferEntity? capturedEntity = null;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((entityId, operation, entity, _) =>
            {
                if (entity is TransferEntity transferEntity)
                {
                    capturedEntity = transferEntity;
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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { SuccessfulItem = _fixture.Create<TransferItem>(), IsSuccess = true }))
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "TransferFile" && payload is TransferFilePayload transferFilePayload)
                {
                    capturedTransferPayloads.Add(transferFilePayload);
                }
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((taskName, payload, options) =>
            {
                if (taskName.Name == "TransferFile")
                {
                    throw exception;
                }

                return (Task<TransferResult>)Task.CompletedTask;
            });

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((taskName, payload, options) =>
            {
                if (taskName.Name == "UpdateTransferStatus" && payload is UpdateTransferStatusPayload statusPayload)
                {
                    capturedStatusPayloads.Add(statusPayload);
                }

                return Task.CompletedTask;
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

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
    public async Task RunOrchestrator_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns((TransferPayload?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _orchestrator.RunOrchestrator(_contextMock.Object));
    }

    [Fact]
    public async Task RunOrchestrator_WhenNetAppToEgress_FiltersDuplicateDestinationFiles()
    {
        // Arrange
        var transferPayload = CreateNetAppToEgressPayloadWithRoot();
        var destinationFiles = new HashSet<string> { "/dest/a.txt" };
        var failedItems = new List<TransferFailedItem>();
        var transferFilePayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()))
            .ReturnsAsync(destinationFiles);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == "TransferFile" && payload is TransferFilePayload transferFilePayload)
                {
                    transferFilePayloads.Add(transferFilePayload);
                }
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && payload is TransferFailedItem failedItem)
                {
                    failedItems.Add(failedItem);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        _contextMock.Verify(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()),
            Times.Once);

        Assert.Single(failedItems);
        Assert.Equal("/root/a.txt", failedItems[0].SourcePath);
        Assert.Equal(TransferErrorCode.FileExists, failedItems[0].ErrorCode);
        Assert.Contains("/dest/a.txt", failedItems[0].ErrorMessage);

        Assert.Single(transferFilePayloads);
        Assert.Equal("/root/b.txt", transferFilePayloads[0].SourcePath.Path);
    }

    [Fact]
    public async Task RunOrchestrator_WhenNetAppToEgress_WithNestedFolders_DetectsDuplicatesForAllFiles()
    {
        // Arrange
        var payload = new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = "uploads/",
            BearerToken = _fixture.Create<string>(),
            SourceRootFolderPath = "folder1",
            SourcePaths =
            [
                new TransferSourcePath
                {
                    Path = "folder1/file1.txt",
                    RelativePath = "folder1/file1.txt"
                },
                new TransferSourcePath
                {
                    Path = "folder1/nestedfolder1/file2.txt",
                    RelativePath = "folder1/nestedfolder1/file2.txt"
                },
                new TransferSourcePath
                {
                    Path = "folder1/nestedfolder1/file3.txt",
                    RelativePath = "folder1/nestedfolder1/file3.txt"
                }
            ],
            CaseId = _fixture.Create<int>(),
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToEgress,
            WorkspaceId = _fixture.Create<string>(),
            BucketName = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = false,
            CorrelationId = _fixture.Create<Guid>()
        };

        var destinationFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "uploads/file1.txt",
            "uploads/nestedfolder1/file2.txt",
            "uploads/nestedfolder1/file3.txt"
        };

        var failedItems = new List<TransferFailedItem>();
        var transferFilePayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(payload);

        _contextMock.Setup(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()))
            .ReturnsAsync(destinationFiles);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, p, _) =>
            {
                if (taskName.Name == "TransferFile" && p is TransferFilePayload tfp)
                    transferFilePayloads.Add(tfp);
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, p, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && p is TransferFailedItem fi)
                    failedItems.Add(fi);
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert — all three files should be flagged as duplicates; no TransferFile calls made.
        Assert.Equal(3, failedItems.Count);
        Assert.All(failedItems, item => Assert.Equal(TransferErrorCode.FileExists, item.ErrorCode));
        Assert.Empty(transferFilePayloads);

        Assert.Contains(failedItems, f => f.SourcePath == "folder1/file1.txt");
        Assert.Contains(failedItems, f => f.SourcePath == "folder1/nestedfolder1/file2.txt");
        Assert.Contains(failedItems, f => f.SourcePath == "folder1/nestedfolder1/file3.txt");
    }

    [Fact]
    public async Task RunOrchestrator_WhenNotNetAppToEgress_SkipsDestinationListing()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        transferPayload.TransferDirection = TransferDirection.EgressToNetApp;

        _contextMock.Setup(c => c.GetInput<TransferPayload>())
            .Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        _contextMock.Verify(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WhenNetAppToEgress_PreCreatesDistinctDestinationFolders()
    {
        // Arrange
        var transferPayload = CreateNetAppToEgressPayloadWithRoot();
        var preCreatePayloads = new List<CreateEgressFoldersPayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync([]);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == nameof(CreateEgressDestinationFolders) && payload is CreateEgressFoldersPayload p)
                    preCreatePayloads.Add(p);
            });

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        var payload = Assert.Single(preCreatePayloads);
        Assert.Equal(transferPayload.WorkspaceId, payload.WorkspaceId);
        Assert.NotEmpty(payload.FolderPaths);
        Assert.Equal(payload.FolderPaths.Count, payload.FolderPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public async Task RunOrchestrator_WhenPreCreateFoldersFailsAfterRetries_ContinuesTransferInsteadOfFailing()
    {
        // Arrange
        var transferPayload = CreateNetAppToEgressPayloadWithRoot();
        var transferFileCalled = false;

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == "ListDestinationFilePaths"),
                It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync([]);

        _contextMock.Setup(c => c.CallActivityAsync(
                It.Is<TaskName>(t => t.Name == nameof(CreateEgressDestinationFolders)),
                It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ThrowsAsync(new InvalidOperationException("Egress unavailable"));

        _contextMock.Setup(c => c.CallActivityAsync(
                It.Is<TaskName>(t => t.Name != nameof(CreateEgressDestinationFolders)),
                It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) =>
            {
                if (taskName.Name == "TransferFile") transferFileCalled = true;
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.True(transferFileCalled);
        _contextMock.Verify(c => c.CallActivityAsync(
                It.Is<TaskName>(t => t.Name == "UpdateTransferStatus"),
                It.Is<object>(o => ((UpdateTransferStatusPayload)o).Status == TransferStatus.Failed),
                It.IsAny<TaskOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WhenTransientFailureIsRetried_EmitsRetryStateAroundBackoffTimer()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var now = new DateTime(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc);
        var callOrder = new List<string>();
        var retryStates = new List<TransferRetryState>();
        DateTime? timerFireAt = null;

        _contextMock.Setup(c => c.CurrentUtcDateTime).Returns(now);
        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.SetupSequence(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult
            {
                IsSuccess = false,
                FailedItem = new TransferFailedItem
                {
                    SourcePath = transferPayload.SourcePaths[0].Path,
                    ErrorCode = TransferErrorCode.Transient,
                    ErrorMessage = "S3 500"
                }
            })
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() });

        _contextMock.Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<DateTime, CancellationToken>((fireAt, _) =>
            {
                timerFireAt = fireAt;
                callOrder.Add("CreateTimer");
            });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation is nameof(TransferEntityState.UpdateRetryState) or nameof(TransferEntityState.ClearRetryState))
                {
                    callOrder.Add(operation);
                }

                if (payload is TransferRetryState retryState)
                {
                    retryStates.Add(retryState);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Equal(
            new[]
            {
                nameof(TransferEntityState.UpdateRetryState),
                "CreateTimer",
                nameof(TransferEntityState.UpdateRetryState),
                nameof(TransferEntityState.ClearRetryState)
            },
            callOrder);

        Assert.Equal(2, retryStates.Count);

        var waiting = retryStates[0];
        Assert.Equal(1, waiting.RetryAttempt);
        Assert.Equal(3, waiting.MaxRetryAttempts);
        Assert.Equal(1, waiting.RetryingFileCount);
        Assert.Equal(60, waiting.RetryDelaySeconds);
        Assert.Equal(now.AddSeconds(60), waiting.NextRetryAt);

        var executing = retryStates[1];
        Assert.Equal(1, executing.RetryAttempt);
        Assert.Equal(1, executing.RetryingFileCount);
        Assert.Null(executing.NextRetryAt);

        Assert.Equal(now.AddSeconds(60), timerFireAt);
    }

    [Fact]
    public async Task RunOrchestrator_WhenTransientFailuresPersist_EmitsRetryStateWithExponentialBackoffPerAttempt()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var now = new DateTime(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc);
        var waitingStates = new List<TransferRetryState>();

        _contextMock.Setup(c => c.CurrentUtcDateTime).Returns(now);
        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult
            {
                IsSuccess = false,
                FailedItem = new TransferFailedItem
                {
                    SourcePath = transferPayload.SourcePaths[0].Path,
                    ErrorCode = TransferErrorCode.Transient,
                    ErrorMessage = "S3 500"
                }
            });

        _contextMock.Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, __, payload, ___) =>
            {
                if (payload is TransferRetryState { NextRetryAt: not null } retryState)
                {
                    waitingStates.Add(retryState);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert -- one waiting state per attempt, with 60/120/240 second backoff
        Assert.Equal(new[] { 1, 2, 3 }, waitingStates.Select(s => s.RetryAttempt));
        Assert.Equal(new[] { 60, 120, 240 }, waitingStates.Select(s => s.RetryDelaySeconds));
        Assert.All(waitingStates, s => Assert.Equal(3, s.MaxRetryAttempts));

        _contextMock.Verify(c => c.Entities.CallEntityAsync(
                It.IsAny<EntityInstanceId>(),
                nameof(TransferEntityState.ClearRetryState),
                It.IsAny<object>(),
                It.IsAny<CallEntityOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WhenNoTransientFailures_DoesNotTouchRetryState()
    {
        // Arrange
        var transferPayload = CreateValidTransferPayload();
        var retryStateOperations = new List<string>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() });

        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, __, ___) =>
            {
                if (operation is nameof(TransferEntityState.UpdateRetryState) or nameof(TransferEntityState.ClearRetryState))
                {
                    retryStateOperations.Add(operation);
                }
            });

        // Act
        await _orchestrator.RunOrchestrator(_contextMock.Object);

        // Assert
        Assert.Empty(retryStateOperations);
    }

    [Fact]
    public async Task RunOrchestrator_WhenAllSourceFilesAreAvailable_DoesNotFailOrWait()
    {
        var transferPayload = CreateValidTransferPayload();
        var failedItems = new List<TransferFailedItem>();
        var transferFilePayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == nameof(TransferFile) && payload is TransferFilePayload tfp)
                    transferFilePayloads.Add(tfp);
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && payload is TransferFailedItem failedItem)
                    failedItems.Add(failedItem);
            });

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.Empty(failedItems);
        Assert.Single(transferFilePayloads);
        _contextMock.Verify(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.Is<TaskName>(t => t.Name == nameof(ValidateSourceFiles)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WhenSourceFilesAppearOnLaterAttempt_WaitsThenTransfers()
    {
        var transferPayload = CreateValidTransferPayload();
        var now = new DateTime(2026, 8, 27, 10, 0, 0, DateTimeKind.Utc);
        DateTime? timerFireAt = null;
        var transferFilePayloads = new List<TransferFilePayload>();
        _sizeConfig.SourceValidationRetryAttempts = 3;
        _sizeConfig.SourceValidationRetryIntervalSeconds = 10;

        _contextMock.Setup(c => c.CurrentUtcDateTime).Returns(now);
        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);

        _contextMock.SetupSequence(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new ValidateSourceFilesResult { Missing = [.. transferPayload.SourcePaths] })
            .ReturnsAsync(new ValidateSourceFilesResult { Available = [.. transferPayload.SourcePaths] });

        _contextMock.Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<DateTime, CancellationToken>((fireAt, _) => timerFireAt = fireAt);

        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == nameof(TransferFile) && payload is TransferFilePayload tfp)
                    transferFilePayloads.Add(tfp);
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.Equal(now.AddSeconds(10), timerFireAt);
        Assert.Single(transferFilePayloads);
        _contextMock.Verify(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.Entities.CallEntityAsync(
                It.IsAny<EntityInstanceId>(),
                nameof(TransferEntityState.UpdateRetryState),
                It.IsAny<object>(),
                It.IsAny<CallEntityOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WhenSourceFilesRemainMissing_FailsAsSourceFileNotFoundAndDoesNotTransfer()
    {
        var transferPayload = CreateValidTransferPayload();
        var failedItems = new List<TransferFailedItem>();
        var transferFileCalled = false;
        _sizeConfig.SourceValidationRetryAttempts = 3;
        _sizeConfig.SourceValidationRetryIntervalSeconds = 10;

        _contextMock.Setup(c => c.CurrentUtcDateTime).Returns(new DateTime(2026, 8, 27, 10, 0, 0, DateTimeKind.Utc));
        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new ValidateSourceFilesResult { Missing = [.. transferPayload.SourcePaths] });
        _contextMock.Setup(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) =>
            {
                if (taskName.Name == nameof(TransferFile)) transferFileCalled = true;
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && payload is TransferFailedItem failedItem)
                    failedItems.Add(failedItem);
            });

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.False(transferFileCalled);
        var failed = Assert.Single(failedItems);
        Assert.Equal(transferPayload.SourcePaths[0].Path, failed.SourcePath);
        Assert.Equal(TransferErrorCode.SourceFileNotFound, failed.ErrorCode);
        Assert.Contains("could not be found", failed.ErrorMessage);
        _contextMock.Verify(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _contextMock.Verify(c => c.Entities.CallEntityAsync(
                It.IsAny<EntityInstanceId>(),
                nameof(TransferEntityState.UpdateRetryState),
                It.IsAny<object>(),
                It.IsAny<CallEntityOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WhenMixedSourceAvailability_TransfersAvailableAndFailsMissing()
    {
        var transferPayload = CreateTransferPayloadWithMultiplePaths();
        var available = transferPayload.SourcePaths[0];
        var missing = transferPayload.SourcePaths[1];
        var failedItems = new List<TransferFailedItem>();
        var transferFilePayloads = new List<TransferFilePayload>();
        _sizeConfig.SourceValidationRetryAttempts = 1;

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new ValidateSourceFilesResult
            {
                Available = [available],
                Missing = [missing]
            });
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, payload, _) =>
            {
                if (taskName.Name == nameof(TransferFile) && payload is TransferFilePayload tfp)
                    transferFilePayloads.Add(tfp);
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && payload is TransferFailedItem failedItem)
                    failedItems.Add(failedItem);
            });

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        var failed = Assert.Single(failedItems);
        Assert.Equal(missing.Path, failed.SourcePath);
        Assert.Equal(TransferErrorCode.SourceFileNotFound, failed.ErrorCode);
        var transferred = Assert.Single(transferFilePayloads);
        Assert.Equal(available.Path, transferred.SourcePath.Path);
        _contextMock.Verify(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WhenSourceAccessFails_FailsFastWithoutPolling()
    {
        var transferPayload = CreateValidTransferPayload();
        var failedItems = new List<TransferFailedItem>();
        var transferFileCalled = false;
        _sizeConfig.SourceValidationRetryAttempts = 5;

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(transferPayload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new ValidateSourceFilesResult
            {
                Failed =
                [
                    new TransferFailedItem
                    {
                        SourcePath = transferPayload.SourcePaths[0].Path,
                        ErrorCode = TransferErrorCode.GeneralError,
                        ErrorMessage = TransferErrorMessages.GetUserMessage(TransferErrorCode.GeneralError)
                    }
                ]
            });
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() })
            .Callback<TaskName, object, TaskOptions>((taskName, _, __) =>
            {
                if (taskName.Name == nameof(TransferFile)) transferFileCalled = true;
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, operation, payload, __) =>
            {
                if (operation == nameof(TransferEntityState.AddFailedItem) && payload is TransferFailedItem failedItem)
                    failedItems.Add(failedItem);
            });

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.False(transferFileCalled);
        var failed = Assert.Single(failedItems);
        Assert.Equal(TransferErrorCode.GeneralError, failed.ErrorCode);
        _contextMock.Verify(c => c.CreateTimer(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.Is<TaskName>(t => t.Name == nameof(ValidateSourceFiles)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WhenAllDestinationDuplicates_SkipsSourceValidation()
    {
        var payload = new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = "uploads/",
            BearerToken = _fixture.Create<string>(),
            SourceRootFolderPath = "folder1",
            SourcePaths =
            [
                new TransferSourcePath { Path = "folder1/file1.txt", RelativePath = "folder1/file1.txt" }
            ],
            CaseId = _fixture.Create<int>(),
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToEgress,
            WorkspaceId = _fixture.Create<string>(),
            BucketName = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = false,
            CorrelationId = _fixture.Create<Guid>()
        };

        _contextMock.Setup(c => c.GetInput<TransferPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync<HashSet<string>>(
                It.Is<TaskName>(t => t.Name == nameof(ListDestinationFilePaths)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()))
            .ReturnsAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "uploads/file1.txt" });
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true, SuccessfulItem = _fixture.Create<TransferItem>() });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync<ValidateSourceFilesResult>(
                It.Is<TaskName>(t => t.Name == nameof(ValidateSourceFiles)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()),
            Times.Never);
    }

    private TransferPayload CreateValidTransferPayload()
    {
        return new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = _fixture.Create<string>(),
            BearerToken = _fixture.Create<string>(),
            SourcePaths =
            [
                new TransferSourcePath
                {
                    Path = _fixture.Create<string>(),
                }
            ],
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            TransferDirection = TransferDirection.EgressToNetApp,
            WorkspaceId = _fixture.Create<string>(),
            BucketName = _fixture.Create<string>(),
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
            BearerToken = _fixture.Create<string>(),
            SourcePaths =
            [
                new() {
                    Path = _fixture.Create<string>(),
                },

                new() {
                    Path = _fixture.Create<string>(),
                }
            ],
            CaseId = _fixture.Create<int>(),
            TransferType = _fixture.Create<TransferType>(),
            TransferDirection = TransferDirection.EgressToNetApp,
            WorkspaceId = _fixture.Create<string>(),
            BucketName = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = _fixture.Create<bool>()
        };
    }

    private TransferPayload CreateNetAppToEgressPayloadWithRoot()
    {
        return new TransferPayload
        {
            TransferId = _fixture.Create<Guid>(),
            DestinationPath = "/dest/",
            BearerToken = _fixture.Create<string>(),
            SourceRootFolderPath = "/root/",
            SourcePaths =
            [
                new TransferSourcePath
                {
                    Path = "/root/a.txt",
                    RelativePath = "/root/a.txt"
                },
                new TransferSourcePath
                {
                    Path = "/root/b.txt",
                    RelativePath = "/root/b.txt"
                }
            ],
            CaseId = _fixture.Create<int>(),
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToEgress,
            WorkspaceId = _fixture.Create<string>(),
            BucketName = _fixture.Create<string>(),
            UserName = _fixture.Create<string>(),
            IsRetry = _fixture.Create<bool>(),
            CorrelationId = _fixture.Create<Guid>()
        };
    }

}

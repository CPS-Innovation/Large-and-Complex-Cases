using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Orchestration;

public class MoveOrchestratorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<TaskOrchestrationContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<SizeConfig>> _sizeConfigMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly MoveOrchestrator _orchestrator;

    public MoveOrchestratorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _contextMock = new Mock<TaskOrchestrationContext>();
        _loggerMock = new Mock<ILogger>();
        _sizeConfigMock = new Mock<IOptions<SizeConfig>>();
        _telemetryClientMock = new Mock<ITelemetryClient>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _sizeConfigMock.Setup(x => x.Value).Returns(new SizeConfig { BatchSize = 10, MaxOrchestratorRetries = 3 });

        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _orchestrator = new MoveOrchestrator(_sizeConfigMock.Object, _telemetryClientMock.Object, _initializationHandlerMock.Object);
    }

    private MoveBatchPayload CreateValidPayload(int fileCount = 1)
    {
        var files = Enumerable.Range(1, fileCount)
            .Select(i => new MoveFileItem
            {
                SourceKey = $"CaseRoot/Source/file{i}.txt",
                DestinationPrefix = "CaseRoot/Dest/",
                DestinationFileName = $"file{i}.txt",
            }).ToList();

        return new MoveBatchPayload
        {
            TransferId = _fixture.Create<Guid>(),
            CaseId = 1,
            UserName = "testuser",
            CorrelationId = _fixture.Create<Guid>(),
            BearerToken = "bearer-token",
            BucketName = "test-bucket",
            Files = files,
            OriginalOperations = files.Select(f => new MoveBatchOriginalOperation { Type = "Material", SourcePath = f.SourceKey, DestinationPrefix = f.DestinationPrefix }).ToList(),
            ManageMaterialsOperationId = _fixture.Create<Guid>(),
        };
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsTransferFileForEachFile()
    {
        var payload = CreateValidPayload(fileCount: 3);
        var transferFileCallCount = 0;

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }))
            .Callback<TaskName, object, TaskOptions>((name, _, __) =>
            {
                if (name.Name == nameof(TransferFile)) transferFileCallCount++;
            });
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.Equal(3, transferFileCallCount);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsDeleteNetAppFiles()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync<List<DeletionError>>(
            It.Is<TaskName>(n => n.Name == nameof(DeleteNetAppFiles)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_DeleteNetAppFilesPayloadHasCorrectIds()
    {
        var payload = CreateValidPayload();
        DeleteNetAppFilesPayload? capturedPayload = null;

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()))
            .Callback<TaskName, object, TaskOptions>((name, arg, _) =>
            {
                if (name.Name == nameof(DeleteNetAppFiles) && arg is DeleteNetAppFilesPayload p)
                    capturedPayload = p;
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.NotNull(capturedPayload);
        Assert.Equal(payload.TransferId, capturedPayload!.TransferId);
        Assert.Equal(payload.BearerToken, capturedPayload.BearerToken);
        Assert.Equal(payload.BucketName, capturedPayload.BucketName);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_NeverCallsDeleteFiles()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(DeleteFiles)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Never);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsRemoveActiveManageMaterialsOperation()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(RemoveActiveManageMaterialsOperation)),
            payload.ManageMaterialsOperationId,
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsFinalizeTransfer()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(FinalizeTransfer)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsWriteMoveActivityLog()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(WriteMoveActivityLog)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_InitializesTransferEntityWithMoveType()
    {
        var payload = CreateValidPayload();
        TransferEntity? capturedEntity = null;

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(
            It.IsAny<EntityInstanceId>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CallEntityOptions>()))
            .Callback<EntityInstanceId, string, object, CallEntityOptions>((_, op, entity, ___) =>
            {
                if (op == nameof(TransferEntityState.Initialize) && entity is TransferEntity te)
                    capturedEntity = te;
            })
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.NotNull(capturedEntity);
        Assert.Equal(Common.Models.Domain.Enums.TransferDirection.NetAppToNetApp, capturedEntity!.Direction);
        Assert.Equal(Common.Models.Domain.Enums.TransferType.Move, capturedEntity.TransferType);
        Assert.Equal(payload.Files.Count, capturedEntity.TotalFiles);
    }

    [Fact]
    public async Task RunOrchestrator_WithNullInput_ThrowsArgumentNullException()
    {
        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns((MoveBatchPayload?)null);
        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _orchestrator.RunOrchestrator(_contextMock.Object));
    }

    [Fact]
    public async Task RunOrchestrator_WhenTransferFileFails_StillCallsRemoveManageMaterialsOperation()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .ThrowsAsync(new Exception("orchestration failure"));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<Exception>(() => _orchestrator.RunOrchestrator(_contextMock.Object));

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(RemoveActiveManageMaterialsOperation)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_EachTransferFilePayloadHasCorrectDestinationPath()
    {
        var file1 = new MoveFileItem { SourceKey = "CaseRoot/Folder-A/report.pdf", DestinationPrefix = "CaseRoot/Folder-B/", DestinationFileName = "report.pdf" };
        var file2 = new MoveFileItem { SourceKey = "CaseRoot/Old/sub/doc.docx", DestinationPrefix = "CaseRoot/New/Old/", DestinationFileName = "sub/doc.docx" };

        var payload = new MoveBatchPayload
        {
            TransferId = _fixture.Create<Guid>(),
            CaseId = 1,
            UserName = "u",
            BearerToken = "b",
            BucketName = "bkt",
            Files = [file1, file2],
            OriginalOperations = [new() { Type = "Material", SourcePath = file1.SourceKey, DestinationPrefix = file1.DestinationPrefix }],
            ManageMaterialsOperationId = _fixture.Create<Guid>(),
        };

        var capturedPayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(
            It.IsAny<TaskName>(),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }))
            .Callback<TaskName, object, TaskOptions>((name, p, _) =>
            {
                if (name.Name == nameof(TransferFile) && p is TransferFilePayload tfp)
                    capturedPayloads.Add(tfp);
            });
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>()));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.Equal(2, capturedPayloads.Count);

        var p1 = capturedPayloads.First(p => p.SourcePath.Path == file1.SourceKey);
        Assert.Equal(file1.DestinationPrefix, p1.DestinationPath);
        Assert.Equal(file1.DestinationFileName, p1.SourcePath.ModifiedPath);

        var p2 = capturedPayloads.First(p => p.SourcePath.Path == file2.SourceKey);
        Assert.Equal(file2.DestinationPrefix, p2.DestinationPath);
        Assert.Equal(file2.DestinationFileName, p2.SourcePath.ModifiedPath);
    }

    [Fact]
    public async Task RunOrchestrator_WhenDeleteNetAppFilesReturnsDeletionErrors_ActivityLogExcludesFailedDeleteKeys()
    {
        // file1 copied and deleted successfully → should appear as "Moved"
        // file2 copied successfully but source delete failed (e.g. locked) → must NOT appear as "Moved"
        var file1 = new MoveFileItem { SourceKey = "CaseRoot/A/file1.pdf", DestinationPrefix = "CaseRoot/B/", DestinationFileName = "file1.pdf" };
        var file2 = new MoveFileItem { SourceKey = "CaseRoot/A/file2.pdf", DestinationPrefix = "CaseRoot/B/", DestinationFileName = "file2.pdf" };

        var payload = new MoveBatchPayload
        {
            TransferId = _fixture.Create<Guid>(),
            CaseId = 1,
            UserName = "testuser",
            CorrelationId = _fixture.Create<Guid>(),
            BearerToken = "bearer-token",
            BucketName = "test-bucket",
            Files = [file1, file2],
            OriginalOperations =
            [
                new() { Type = "Material", SourcePath = file1.SourceKey, DestinationPrefix = file1.DestinationPrefix },
                new() { Type = "Material", SourcePath = file2.SourceKey, DestinationPrefix = file2.DestinationPrefix },
            ],
            ManageMaterialsOperationId = _fixture.Create<Guid>(),
        };

        // Both copies succeed
        _contextMock.Setup(c => c.GetInput<MoveBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns<TaskName, object, TaskOptions>((name, p, _) =>
            {
                if (name.Name == nameof(TransferFile) && p is TransferFilePayload tfp)
                        return Task.FromResult(new TransferResult
                    {
                        IsSuccess = true,
                        SuccessfulItem = new TransferItem { SourcePath = tfp.SourcePath.Path, Status = Models.Domain.Enums.TransferItemStatus.Completed, Size = 0, IsRenamed = false }
                    });
                return Task.FromResult(new TransferResult { IsSuccess = true });
            });

        // Delete phase returns a failure for file2 (simulates a locked file)
        _contextMock.Setup(c => c.CallActivityAsync<List<DeletionError>>(
                It.Is<TaskName>(n => n.Name == nameof(DeleteNetAppFiles)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new List<DeletionError>
            {
                new() { FileId = file2.SourceKey, ErrorMessage = "File is locked via SMB and could not be deleted." }
            }));

        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        WriteMoveActivityLogPayload? capturedActivityLog = null;
        _contextMock.Setup(c => c.CallActivityAsync(
                It.Is<TaskName>(n => n.Name == nameof(WriteMoveActivityLog)),
                It.IsAny<object>(),
                It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask)
            .Callback<TaskName, object, TaskOptions>((_, arg, _) =>
            {
                if (arg is WriteMoveActivityLogPayload p)
                    capturedActivityLog = p;
            });

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.NotNull(capturedActivityLog);
        Assert.Contains(file1.SourceKey, capturedActivityLog!.SuccessfulSourceKeys);
        Assert.DoesNotContain(file2.SourceKey, capturedActivityLog.SuccessfulSourceKeys);
    }
}

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

public class CopyOrchestratorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<TaskOrchestrationContext> _contextMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<SizeConfig>> _sizeConfigMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly CopyOrchestrator _orchestrator;

    public CopyOrchestratorTests()
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

        _orchestrator = new CopyOrchestrator(_sizeConfigMock.Object, _telemetryClientMock.Object, _initializationHandlerMock.Object);
    }

    private CopyBatchPayload CreateValidPayload(int fileCount = 1)
    {
        var files = Enumerable.Range(1, fileCount)
            .Select(i => new CopyFileItem
            {
                SourceKey = $"CaseRoot/Source/file{i}.txt",
                DestinationPrefix = "CaseRoot/Dest/",
                DestinationFileName = $"file{i}.txt",
            }).ToList();

        return new CopyBatchPayload
        {
            TransferId = _fixture.Create<Guid>(),
            CaseId = 1,
            UserName = "testuser",
            CorrelationId = _fixture.Create<Guid>(),
            BearerToken = "bearer-token",
            BucketName = "test-bucket",
            Files = files,
            OriginalOperations = files.Select(f => new CopyBatchOriginalOperation { Type = "Material", SourcePath = f.SourceKey }).ToList(),
            ManageMaterialsOperationId = _fixture.Create<Guid>(),
        };
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsTransferFileForEachFile()
    {
        var payload = CreateValidPayload(fileCount: 3);
        var transferFileCallCount = 0;

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }))
            .Callback<TaskName, object, TaskOptions>((name, _, __) =>
            {
                if (name.Name == nameof(TransferFile)) transferFileCallCount++;
            });
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        Assert.Equal(3, transferFileCallCount);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_NeverCallsDeleteFiles()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
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

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
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

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(FinalizeTransfer)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_CallsWriteCopyActivityLog()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
        _contextMock.Setup(c => c.Entities.CallEntityAsync(It.IsAny<EntityInstanceId>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);

        await _orchestrator.RunOrchestrator(_contextMock.Object);

        _contextMock.Verify(c => c.CallActivityAsync(
            It.Is<TaskName>(n => n.Name == nameof(WriteCopyActivityLog)),
            It.IsAny<object>(),
            It.IsAny<TaskOptions>()), Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_WithValidInput_InitializesTransferEntityWithCopyDirection()
    {
        var payload = CreateValidPayload();
        TransferEntity? capturedEntity = null;

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
        _contextMock.Setup(c => c.CallActivityAsync(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.CallActivityAsync<TransferResult>(It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<TaskOptions>()))
            .Returns(Task.FromResult(new TransferResult { IsSuccess = true }));
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
        Assert.Equal(Common.Models.Domain.Enums.TransferDirection.NetAppToNetApp, capturedEntity.Direction);
        Assert.Equal(Common.Models.Domain.Enums.TransferType.Copy, capturedEntity.TransferType);
        Assert.Equal(payload.Files.Count, capturedEntity.TotalFiles);
    }

    [Fact]
    public async Task RunOrchestrator_WithNullInput_ThrowsArgumentNullException()
    {
        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns((CopyBatchPayload?)null);
        _contextMock.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _orchestrator.RunOrchestrator(_contextMock.Object));
    }

    [Fact]
    public async Task RunOrchestrator_WhenTransferFileFails_StillCallsRemoveManageMaterialsOperation()
    {
        var payload = CreateValidPayload();

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
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
        var file1 = new CopyFileItem { SourceKey = "CaseRoot/Folder-A/report.pdf", DestinationPrefix = "CaseRoot/Folder-B/", DestinationFileName = "report.pdf" };
        var file2 = new CopyFileItem { SourceKey = "CaseRoot/Old/sub/doc.docx", DestinationPrefix = "CaseRoot/New/Old/", DestinationFileName = "sub/doc.docx" };

        var payload = new CopyBatchPayload
        {
            TransferId = _fixture.Create<Guid>(),
            CaseId = 1,
            UserName = "u",
            BearerToken = "b",
            BucketName = "bkt",
            Files = [file1, file2],
            OriginalOperations = [new() { Type = "Material", SourcePath = file1.SourceKey }],
            ManageMaterialsOperationId = _fixture.Create<Guid>(),
        };

        var capturedPayloads = new List<TransferFilePayload>();

        _contextMock.Setup(c => c.GetInput<CopyBatchPayload>()).Returns(payload);
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
}

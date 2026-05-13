using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;
using Amazon.S3;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class DeleteNetAppFilesTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ITransferEntityHelper> _transferEntityHelperMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ILogger<DeleteNetAppFiles>> _loggerMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly DeleteNetAppFiles _activity;
    private readonly Guid _transferId;
    private const string BearerToken = "test-bearer";
    private const string BucketName = "test-bucket";
    private const string UserName = "testuser";

    public DeleteNetAppFilesTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _transferEntityHelperMock = new Mock<ITransferEntityHelper>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _loggerMock = new Mock<ILogger<DeleteNetAppFiles>>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _telemetryClientMock = new Mock<ITelemetryClient>();

        _transferId = _fixture.Create<Guid>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string op, string path, bool isFolder) =>
                new DeleteFileOrFolderArg { BearerToken = bearer, BucketName = bucket, OperationName = op, Path = path });

        _transferEntityHelperMock
            .Setup(h => h.DeleteMovedItemsCompleted(It.IsAny<Guid>(), It.IsAny<List<DeletionError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _activity = new DeleteNetAppFiles(
            _transferEntityHelperMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _loggerMock.Object,
            _initializationHandlerMock.Object,
            _telemetryClientMock.Object);
    }

    [Fact]
    public async Task Run_WhenPayloadIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _activity.Run(null, CancellationToken.None));
    }

    [Fact]
    public async Task Run_WhenEntityNotFound_ThrowsInvalidOperationException()
    {
        var payload = CreatePayload();

        _transferEntityHelperMock
            .Setup(h => h.GetTransferEntityAsync(payload.TransferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityMetadata<TransferEntity>?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _activity.Run(payload, CancellationToken.None));
    }

    [Fact]
    public async Task Run_WhenNoSuccessfulItems_DoesNotCallDelete()
    {
        var payload = CreatePayload();
        SetupEntity(payload.TransferId, []);

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenNoSuccessfulItems_CallsDeleteMovedItemsCompletedWithEmptyList()
    {
        var payload = CreatePayload();
        SetupEntity(payload.TransferId, []);

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(
                payload.TransferId,
                It.Is<List<DeletionError>>(l => l.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenFilesSuccessfullyDeleted_CallsDeleteForEachSuccessfulItem()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/file1.pdf", FileId = "f1", Size = 100, IsRenamed = false },
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/file2.pdf", FileId = "f2", Size = 200, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Run_WhenAllFilesDeleted_CallsDeleteMovedItemsCompletedWithEmptyErrors()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/file1.pdf", FileId = "f1", Size = 100, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(
                payload.TransferId,
                It.Is<List<DeletionError>>(l => l.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteReturnsUnsuccessfulResult_AddsToFailedItems()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/locked.pdf", FileId = "f1", Size = 100, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(false, true, 0, "Permission denied", 403));

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(
                payload.TransferId,
                It.Is<List<DeletionError>>(l =>
                    l.Count == 1 &&
                    l[0].FileId == "CaseRoot/locked.pdf"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteThrows423SmbLockException_AddsToFailedItemsAndContinues()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/locked.pdf", FileId = "f1", Size = 100, IsRenamed = false },
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/free.pdf", FileId = "f2", Size = 200, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "CaseRoot/locked.pdf")))
            .ThrowsAsync(new AmazonS3Exception("locked", ErrorType.Sender, "Locked", "req1", (System.Net.HttpStatusCode)423));

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "CaseRoot/free.pdf")))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(
                payload.TransferId,
                It.Is<List<DeletionError>>(l =>
                    l.Count == 1 &&
                    l[0].FileId == "CaseRoot/locked.pdf" &&
                    l[0].ErrorMessage.Contains("SMB")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteThrows423SmbLockException_DoesNotPropagateException()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/locked.pdf", FileId = "f1", Size = 100, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ThrowsAsync(new AmazonS3Exception("locked", ErrorType.Sender, "Locked", "req1", (System.Net.HttpStatusCode)423));

        var ex = await Record.ExceptionAsync(() => _activity.Run(payload, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Run_WhenDeleteThrowsUnexpectedException_AddsToFailedItemsAndContinues()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/problem.pdf", FileId = "f1", Size = 100, IsRenamed = false },
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/ok.pdf", FileId = "f2", Size = 200, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "CaseRoot/problem.pdf")))
            .ThrowsAsync(new Exception("Unexpected error"));

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == "CaseRoot/ok.pdf")))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(
                payload.TransferId,
                It.Is<List<DeletionError>>(l =>
                    l.Count == 1 &&
                    l[0].FileId == "CaseRoot/problem.pdf" &&
                    l[0].ErrorMessage.Contains("Unexpected error")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenDeleteThrowsUnexpectedException_DoesNotPropagateException()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/problem.pdf", FileId = "f1", Size = 100, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ThrowsAsync(new Exception("Unexpected storage failure"));

        var ex = await Record.ExceptionAsync(() => _activity.Run(payload, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Run_AlwaysCallsDeleteMovedItemsCompleted()
    {
        var payload = CreatePayload();
        var items = new List<TransferItem>
        {
            new() { Status = TransferItemStatus.Completed, SourcePath = "CaseRoot/file.pdf", FileId = "f1", Size = 100, IsRenamed = false },
        };
        SetupEntity(payload.TransferId, items);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ThrowsAsync(new AmazonS3Exception("locked", ErrorType.Sender, "Locked", "req1", (System.Net.HttpStatusCode)423));

        await _activity.Run(payload, CancellationToken.None);

        _transferEntityHelperMock.Verify(
            h => h.DeleteMovedItemsCompleted(payload.TransferId, It.IsAny<List<DeletionError>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private DeleteNetAppFilesPayload CreatePayload() => new()
    {
        TransferId = _transferId,
        BearerToken = BearerToken,
        BucketName = BucketName,
        UserName = UserName,
        CaseId = 99,
    };

    private void SetupEntity(Guid transferId, List<TransferItem> successfulItems)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());
        var entity = new EntityMetadata<TransferEntity>(
            entityId,
            new TransferEntity
            {
                SuccessfulItems = successfulItems,
                DestinationPath = "CaseRoot/Dest/",
                BearerToken = BearerToken,
            });

        _transferEntityHelperMock
            .Setup(h => h.GetTransferEntityAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }
}

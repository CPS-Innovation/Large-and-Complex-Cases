using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class DeleteFilesTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock;
    private readonly Mock<IStorageClient> _storageClientMock;
    private readonly Mock<ILogger<DeleteFiles>> _loggerMock;
    private readonly Mock<ITransferEntityHelper> _transferEntityHelperMock;
    private readonly DeleteFiles _activity;
    private readonly string _workspaceId;
    private readonly string _destinationPath;

    public DeleteFilesTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _storageClientFactoryMock = new Mock<IStorageClientFactory>();
        _storageClientMock = new Mock<IStorageClient>();
        _loggerMock = new Mock<ILogger<DeleteFiles>>();
        _transferEntityHelperMock = new Mock<ITransferEntityHelper>();

        _workspaceId = _fixture.Create<string>();
        _destinationPath = _fixture.Create<string>();

        _activity = new DeleteFiles(_transferEntityHelperMock.Object, _storageClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Run_ThrowsArgumentNullException_WhenPayloadIsNull()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _activity.Run(null, CancellationToken.None));
    }

    [Fact]
    public async Task Run_ThrowsArgumentException_WhenTransferDirectionIsNetAppToEgress()
    {
        // Arrange
        var payload = new DeleteFilesPayload
        {
            TransferDirection = TransferDirection.NetAppToEgress,
            TransferId = Guid.NewGuid()
        };

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _activity.Run(payload, CancellationToken.None));
    }

    [Fact]
    public async Task Run_ThrowsInvalidOperationException_WhenEntityNotFound()
    {
        // Arrange
        var payload = new DeleteFilesPayload
        {
            TransferDirection = TransferDirection.EgressToNetApp,
            TransferId = Guid.NewGuid()
        };

        _transferEntityHelperMock
            .Setup(c => c.GetTransferEntityAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityMetadata<TransferEntity>?)null);

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _activity.Run(payload, CancellationToken.None));
    }

    [Fact]
    public async Task Run_LogsAndReturns_WhenNoFilesToDelete()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();
        var expectedPartialErrorMessage = "No files";

        var payload = new DeleteFilesPayload
        {
            TransferDirection = TransferDirection.EgressToNetApp,
            TransferId = transferId
        };

        var entity = new EntityMetadata<TransferEntity>(
             id: new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString()),
             state: new TransferEntity
             {
                 SuccessfulItems = [],
                 DestinationPath = _destinationPath,
             }
         );

        _transferEntityHelperMock
            .Setup(c => c.GetTransferEntityAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        await _activity.Run(payload, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedPartialErrorMessage) && v.ToString().Contains(transferId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_CallsDeleteFilesAsync_WhenFilesToDeleteExist()
    {
        // Arrange
        var payload = new DeleteFilesPayload
        {
            TransferDirection = TransferDirection.EgressToNetApp,
            TransferId = Guid.NewGuid(),
            WorkspaceId = _workspaceId
        };

        var successfulItems = new List<TransferItem>
        {
            new() {
                Status = TransferItemStatus.Completed,
                SourcePath = "file1.txt",
                FileId = "f1",
                Size = 1234,
                IsRenamed = false
            },
            new() {
                Status = TransferItemStatus.Completed,
                SourcePath = "file2.txt",
                FileId = "f2",
                Size = 5678,
                IsRenamed = false
            }
        };

        var entity = new EntityMetadata<TransferEntity>(
            id: new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString()),
            state: new TransferEntity
            {
                SuccessfulItems = successfulItems,
                DestinationPath = _destinationPath,
            }
        );

        _transferEntityHelperMock
             .Setup(c => c.GetTransferEntityAsync(
                 It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        _transferEntityHelperMock
            .Setup(c => c.DeleteMovedItemsCompleted(
                It.IsAny<Guid>(), It.IsAny<List<FailedToDeleteItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storageClientFactoryMock
            .Setup(f => f.GetSourceClientForDirection(payload.TransferDirection))
            .Returns(_storageClientMock.Object);

        _storageClientMock
            .Setup(c => c.DeleteFilesAsync(
                It.IsAny<List<DeletionEntityDto>>(),
                payload.WorkspaceId))
            .ReturnsAsync(new DeleteFilesResult());

        // Act
        await _activity.Run(payload, CancellationToken.None);

        // Assert
        _storageClientMock.Verify(
            c => c.DeleteFilesAsync(
                It.Is<List<DeletionEntityDto>>(l =>
                    l.Count == 2 &&
                    l.Any(x => x.Path == "file1.txt" && x.FileId == "f1") &&
                    l.Any(x => x.Path == "file2.txt" && x.FileId == "f2")),
                payload.WorkspaceId),
            Times.Once);
    }

    [Fact]
    public async Task Run_LogsError_WhenDeleteFilesAsyncThrowsException()
    {
        // Arrange
        var payload = new DeleteFilesPayload
        {
            TransferId = Guid.NewGuid(),
            TransferDirection = TransferDirection.EgressToNetApp,
            WorkspaceId = _workspaceId
        };

        var successfulItems = new List<TransferItem>
        {
            new() {
                Status = TransferItemStatus.Completed,
                SourcePath = "file1.txt",
                FileId = "f1",
                Size = 1234,
                IsRenamed = false
            },
            new() {
                Status = TransferItemStatus.Completed,
                SourcePath = "file2.txt",
                FileId = "f2",
                Size = 5678,
                IsRenamed = false
            }
        };

        var entity = new EntityMetadata<TransferEntity>(
            id: new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString()),
            state: new TransferEntity
            {
                SuccessfulItems = successfulItems,
                DestinationPath = _destinationPath,
            }
        );

        _transferEntityHelperMock
            .Setup(x => x.GetTransferEntityAsync(payload.TransferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _storageClientFactoryMock
            .Setup(x => x.GetSourceClientForDirection(payload.TransferDirection))
            .Returns(_storageClientMock.Object);

        var exception = new Exception("delete failed");
        _storageClientMock
            .Setup(x => x.DeleteFilesAsync(It.IsAny<List<DeletionEntityDto>>(), payload.WorkspaceId))
            .ThrowsAsync(exception);

        var sut = new DeleteFiles(_transferEntityHelperMock.Object, _storageClientFactoryMock.Object, _loggerMock.Object);

        // Act
        await sut.Run(payload, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred while deleting files")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
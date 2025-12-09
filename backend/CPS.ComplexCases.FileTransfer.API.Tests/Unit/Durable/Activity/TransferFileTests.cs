using System.Text;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class TransferFileTests
{
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock = new();
    private readonly Mock<IStorageClient> _sourceClientMock = new();
    private readonly Mock<IStorageClient> _destinationClientMock = new();
    private readonly Mock<ILogger<TransferFile>> _loggerMock = new();
    private readonly IOptions<SizeConfig> _sizeConfig = Options.Create(new SizeConfig { ChunkSizeBytes = 4 });

    private readonly TransferFile _activity;

    public TransferFileTests()
    {
        _activity = new TransferFile(_storageClientFactoryMock.Object, _loggerMock.Object, _sizeConfig);
    }

    private static TransferFilePayload CreatePayload()
    {
        return new TransferFilePayload
        {
            TransferId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid().ToString(),
            SourcePath = new TransferSourcePath
            {
                Path = "source.txt",
                FullFilePath = "source.txt",
                RelativePath = "relative/source.txt",
                FileId = "file-1"
            },
            DestinationPath = "dest",
            TransferDirection = TransferDirection.EgressToNetApp,
            BearerToken = "fakeBearerToken",
        };
    }

    [Fact]
    public async Task Run_SuccessfulTransfer_ReturnsSuccessResult()
    {
        // Arrange
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("testdata"); // 8 bytes => fits in one chunk
        var stream = new MemoryStream(content);

        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(payload.SourcePath.Path,
                                              payload.WorkspaceId,
                                              payload.SourcePath.FileId,
                                              payload.BearerToken))
            .ReturnsAsync(stream);

        _destinationClientMock
            .Setup(x => x.InitiateUploadAsync(
                payload.DestinationPath,
                content.Length,
                payload.SourcePath.Path,
                payload.WorkspaceId,
                payload.SourcePath.RelativePath,
                payload.SourceRootFolderPath,
                payload.BearerToken
            ))
            .ReturnsAsync(session);

        _destinationClientMock
            .Setup(x => x.UploadChunkAsync(
                session,
                1,
                It.IsAny<byte[]>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                payload.BearerToken))
            .ReturnsAsync(new UploadChunkResult(payload.TransferDirection, "etag1", 1));

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.SuccessfulItem);
        Assert.Equal(TransferItemStatus.Completed, result.SuccessfulItem.Status);
        Assert.Equal(payload.SourcePath.FileId, result.SuccessfulItem.FileId);
        Assert.Equal(content.Length, result.SuccessfulItem.Size);
    }

    [Fact]
    public async Task Run_FileExistsException_ReturnsFailedResultWithFileExistsCode()
    {
        // Arrange
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new FileExistsException("File already exists."));

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.FileExists, result.FailedItem.ErrorCode);
    }

    [Fact]
    public async Task Run_UnexpectedException_ReturnsFailedResultWithGeneralError()
    {
        // Arrange
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Contains("System.InvalidOperationException", result.FailedItem.ErrorMessage);
    }

    [Fact]
    public async Task Run_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var payload = CreatePayload();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var content = Encoding.UTF8.GetBytes("testdata");
        var stream = new MemoryStream(content);

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(payload.SourcePath.Path,
                                              payload.WorkspaceId,
                                              payload.SourcePath.FileId,
                                              payload.BearerToken))
            .ReturnsAsync(stream);

        _destinationClientMock
            .Setup(x => x.InitiateUploadAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                payload.BearerToken))
            .ReturnsAsync(new UploadSession { UploadId = Guid.NewGuid().ToString() });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _activity.Run(payload, cts.Token));
    }
}

using System.Text;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Telemetry;
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
    private readonly Mock<IInitializationHandler> _initializationHandlerMock = new();
    private readonly Mock<ITelemetryClient> _telemetryClientMock = new();
    private readonly IOptions<SizeConfig> _sizeConfig = Options.Create(new SizeConfig { ChunkSizeBytes = 4 });

    private readonly TransferFile _activity;

    public TransferFileTests()
    {
        _activity = new TransferFile(_storageClientFactoryMock.Object, _loggerMock.Object, _sizeConfig, _initializationHandlerMock.Object, _telemetryClientMock.Object);
    }

    private static TransferFilePayload CreatePayload()
    {
        return new TransferFilePayload
        {
            CaseId = 123,
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
            BucketName = "test-bucket",
            UserName = "testuser",
            CorrelationId = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task Run_SuccessfulTransfer_ReturnsSuccessResult()
    {
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("testdata");
        var stream = new MemoryStream(content);
        var contentLength = content.Length;

        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(payload.SourcePath.Path,
                                              payload.WorkspaceId,
                                              payload.SourcePath.FileId,
                                              payload.BearerToken,
                                              payload.BucketName))
            .ReturnsAsync((stream, contentLength));

        _destinationClientMock
            .Setup(x => x.InitiateUploadAsync(
                payload.DestinationPath,
                contentLength,
                payload.SourcePath.Path,
                payload.WorkspaceId,
                payload.SourcePath.RelativePath,
                payload.SourceRootFolderPath,
                payload.BearerToken,
                payload.BucketName))
            .ReturnsAsync(session);

        _destinationClientMock
            .Setup(x => x.UploadChunkAsync(
                It.IsAny<UploadSession>(),
                It.IsAny<int>(),
                It.IsAny<byte[]>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync((UploadSession session, int partNum, byte[] data, long start, long end, long total, string token, string? bucket) =>
                new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum));

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken,
                payload.BucketName,
                payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path))
            .Returns(Task.FromResult(true));

        var result = await _activity.Run(payload);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.SuccessfulItem);
        Assert.Equal(TransferItemStatus.Completed, result.SuccessfulItem.Status);
        Assert.Equal(payload.SourcePath.FileId, result.SuccessfulItem.FileId);
        Assert.Equal(contentLength, result.SuccessfulItem.Size);
    }

    [Fact]
    public async Task Run_VerificationFails_ReturnsFailedResultWithIntegrityVerificationFailedCode()
    {
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("testdata");
        var stream = new MemoryStream(content);
        var contentLength = content.Length;

        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(payload.SourcePath.Path,
                                              payload.WorkspaceId,
                                              payload.SourcePath.FileId,
                                              payload.BearerToken,
                                              payload.BucketName))
            .ReturnsAsync((stream, contentLength));

        _destinationClientMock
            .Setup(x => x.InitiateUploadAsync(
                payload.DestinationPath,
                contentLength,
                payload.SourcePath.Path,
                payload.WorkspaceId,
                payload.SourcePath.RelativePath,
                payload.SourceRootFolderPath,
                payload.BearerToken,
                payload.BucketName))
            .ReturnsAsync(session);

        _destinationClientMock
            .Setup(x => x.UploadChunkAsync(
                It.IsAny<UploadSession>(),
                It.IsAny<int>(),
                It.IsAny<byte[]>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync((UploadSession session, int partNum, byte[] data, long start, long end, long total, string token, string? bucket) =>
                new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum));

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken,
                payload.BucketName,
                payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path))
            .Returns(Task.FromResult(false));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.IntegrityVerificationFailed, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
        Assert.Equal("Upload completed but failed to verify.", result.FailedItem.ErrorMessage);
    }

    [Fact]
    public async Task Run_FileExistsException_ReturnsFailedResultWithFileExistsCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new FileExistsException("File already exists."));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.FileExists, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_UnexpectedException_ReturnsFailedResultWithGeneralError()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
        Assert.Contains("System.InvalidOperationException", result.FailedItem.ErrorMessage);
    }

    [Fact]
    public async Task Run_UnexpectedException_UsesPathWhenFullFilePathIsNull()
    {
        var payload = CreatePayload();
        payload.SourcePath.FullFilePath = null;

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.Path, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_EgressToNetApp_WhenDestinationFileExists_ReturnsFileExistsFailure()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _destinationClientMock
            .Setup(x => x.FileExistsAsync(
                payload.DestinationPath + payload.SourcePath.Path,
                payload.WorkspaceId,
                payload.BearerToken,
                payload.BucketName))
            .ReturnsAsync(true);

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.FileExists, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
        Assert.Contains(payload.DestinationPath + payload.SourcePath.Path, result.FailedItem.ErrorMessage);

        _sourceClientMock.Verify(x => x.OpenReadStreamAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_Cancelled_ThrowsOperationCanceledException()
    {
        var payload = CreatePayload();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var content = Encoding.UTF8.GetBytes("testdata");
        var stream = new MemoryStream(content);
        var contentLength = content.Length;

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(payload.SourcePath.Path,
                                              payload.WorkspaceId,
                                              payload.SourcePath.FileId,
                                              payload.BearerToken,
                                              payload.BucketName))
            .ReturnsAsync((stream, contentLength));

        _destinationClientMock
            .Setup(x => x.InitiateUploadAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                payload.BearerToken,
                payload.BucketName))
            .ReturnsAsync(new UploadSession { UploadId = Guid.NewGuid().ToString() });

        await Assert.ThrowsAsync<OperationCanceledException>(() => _activity.Run(payload, cts.Token));
    }
}

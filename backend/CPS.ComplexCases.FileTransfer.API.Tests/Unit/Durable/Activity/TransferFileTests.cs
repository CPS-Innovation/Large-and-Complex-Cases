using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
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
using Moq;
using System.Collections.Concurrent;

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
        _activity = new TransferFile(_storageClientFactoryMock.Object, _loggerMock.Object, _sizeConfig,
            _initializationHandlerMock.Object, _telemetryClientMock.Object);
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
            .ReturnsAsync((UploadSession session, int partNum, byte[] data, long start, long end, long total,
                    string token, string? bucket) =>
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
            .ReturnsAsync((UploadSession session, int partNum, byte[] data, long start, long end, long total,
                    string token, string? bucket) =>
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
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
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
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
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
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
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

        _sourceClientMock.Verify(x => x.InitiateUploadAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>()),
            Times.Never);

        _sourceClientMock.Verify(x => x.OpenReadStreamAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task
        Run_EgressToNetApp_FileExistsCheckThrowsHttpRequestExceptionForServerError_ReturnsTransientFailure()
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
            .ThrowsAsync(new HttpRequestException("Internal Server Error", null, HttpStatusCode.InternalServerError));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
        Assert.Contains("HTTP 500", result.FailedItem.ErrorMessage);

        _sourceClientMock.Verify(x => x.OpenReadStreamAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_EgressToNetApp_FileExistsCheckThrowsUnexpectedException_ReturnsGeneralErrorFailure()
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
            .ThrowsAsync(new InvalidOperationException("File exists check failed"));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
        Assert.Contains("System.InvalidOperationException", result.FailedItem.ErrorMessage);

        _sourceClientMock.Verify(x => x.OpenReadStreamAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_TaskCanceledWithoutCancellationRequest_ReturnsFailedResultWithGeneralError()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new TaskCanceledException("The request was canceled due to timeout."));

        var result = await _activity.Run(payload, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Contains("HTTP request timed out", result.FailedItem.ErrorMessage);
    }

    [Fact]
    public async Task Run_MultipartUpload_WithOversizedArrayPoolBuffer_UploadsOnlyExpectedBytesPerPart()
    {
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("abcde");
        var stream = new MemoryStream(content);
        var contentLength = content.Length;
        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };
        var uploadedParts = new ConcurrentDictionary<int, (byte[] Data, long Start, long End)>();

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
            .ReturnsAsync((UploadSession uploadSession, int partNum, byte[] data, long start, long end, long total,
                string token, string? bucket) =>
            {
                uploadedParts[partNum] = (data, start, end);
                return new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum);
            });

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
        Assert.Equal(2, uploadedParts.Count);

        Assert.True(uploadedParts.TryGetValue(1, out var part1));
        Assert.Equal(0, part1.Start);
        Assert.Equal(3, part1.End);
        Assert.Equal(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' }, part1.Data);

        Assert.True(uploadedParts.TryGetValue(2, out var part2));
        Assert.Equal(4, part2.Start);
        Assert.Equal(4, part2.End);
        Assert.Equal(new byte[] { (byte)'e' }, part2.Data);
    }

    [Fact]
    public async Task Run_MultipartUpload_LimitsMaxConcurrentUploads()
    {
        // Arrange — 20-byte file with 4-byte chunks = 5 parts.
        // MaxConcurrentPartUploads defaults to 4 in SizeConfig.
        // We use TaskCompletionSources to hold each upload in-flight and track
        // the peak number of concurrent uploads. The semaphore should prevent
        // more than 4 from being in-flight at once.
        var payload = CreatePayload();
        var content = new byte[20];
        for (int i = 0; i < content.Length; i++) content[i] = (byte)(i + 1);
        var stream = new MemoryStream(content);
        var contentLength = content.Length;
        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };

        var concurrentCount = 0;
        var peakConcurrent = 0;
        var lockObj = new object();
        var uploadGates = new ConcurrentDictionary<int, TaskCompletionSource<UploadChunkResult>>();

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
            .Returns((UploadSession s, int partNum, byte[] data, long start, long end, long total, string token,
                string? bucket) =>
            {
                lock (lockObj)
                {
                    concurrentCount++;
                    if (concurrentCount > peakConcurrent)
                        peakConcurrent = concurrentCount;
                }

                var tcs = new TaskCompletionSource<UploadChunkResult>();
                uploadGates[partNum] = tcs;
                return tcs.Task.ContinueWith(t =>
                {
                    lock (lockObj)
                    {
                        concurrentCount--;
                    }

                    return t.Result;
                });
            });

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken,
                payload.BucketName,
                payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path))
            .Returns(Task.FromResult(true));

        // Act — start the transfer on a background task
        var transferTask = Task.Run(() => _activity.Run(payload));

        // Wait for uploads to start appearing, then release them one at a time
        // to let the pipeline progress. Poll for gates to appear.
        var releasedParts = new HashSet<int>();
        var maxWait = TimeSpan.FromSeconds(10);
        var deadline = DateTime.UtcNow + maxWait;

        while (releasedParts.Count < 5 && DateTime.UtcNow < deadline)
        {
            foreach (var kvp in uploadGates)
            {
                if (!releasedParts.Contains(kvp.Key))
                {
                    kvp.Value.SetResult(new UploadChunkResult(
                        TransferDirection.EgressToNetApp, $"etag{kvp.Key}", kvp.Key));
                    releasedParts.Add(kvp.Key);
                }
            }

            await Task.Delay(50);
        }

        var result = await transferTask;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, releasedParts.Count);
        Assert.True(peakConcurrent <= 4,
            $"Peak concurrent uploads was {peakConcurrent}, should be <= MaxConcurrentPartUploads (4)");
    }

    [Fact]
    public async Task Run_MultipartUpload_LargeFile_AllPartsUploadedInOrder()
    {
        // Arrange — 20-byte file, 4-byte chunks = 5 parts
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRST"); // exactly 20 bytes
        var stream = new MemoryStream(content);
        var contentLength = content.Length;
        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };
        var uploadedParts = new ConcurrentDictionary<int, (byte[] Data, long Start, long End)>();

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
            .ReturnsAsync((UploadSession s, int partNum, byte[] data, long start, long end, long total, string token,
                string? bucket) =>
            {
                uploadedParts[partNum] = (data, start, end);
                return new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum);
            });

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken,
                payload.BucketName,
                payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, uploadedParts.Count);

        // Verify each part has the correct byte range and data
        Assert.True(uploadedParts.TryGetValue(1, out var p1));
        Assert.Equal(0, p1.Start);
        Assert.Equal(3, p1.End);
        Assert.Equal(Encoding.UTF8.GetBytes("ABCD"), p1.Data);

        Assert.True(uploadedParts.TryGetValue(2, out var p2));
        Assert.Equal(4, p2.Start);
        Assert.Equal(7, p2.End);
        Assert.Equal(Encoding.UTF8.GetBytes("EFGH"), p2.Data);

        Assert.True(uploadedParts.TryGetValue(3, out var p3));
        Assert.Equal(8, p3.Start);
        Assert.Equal(11, p3.End);
        Assert.Equal(Encoding.UTF8.GetBytes("IJKL"), p3.Data);

        Assert.True(uploadedParts.TryGetValue(4, out var p4));
        Assert.Equal(12, p4.Start);
        Assert.Equal(15, p4.End);
        Assert.Equal(Encoding.UTF8.GetBytes("MNOP"), p4.Data);

        Assert.True(uploadedParts.TryGetValue(5, out var p5));
        Assert.Equal(16, p5.Start);
        Assert.Equal(19, p5.End);
        Assert.Equal(Encoding.UTF8.GetBytes("QRST"), p5.Data);
    }

    [Fact]
    public async Task Run_MultipartUpload_UploadFailure_PropagatesAsGeneralError()
    {
        // Arrange — upload of part 2 throws
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("abcdefgh"); // 8 bytes = 2 parts at 4-byte chunks
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

        var callCount = 0;
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
            .ReturnsAsync((UploadSession s, int partNum, byte[] data, long start, long end, long total, string token,
                string? bucket) =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                    throw new InvalidOperationException("Simulated upload failure on part 2");
                return new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum);
            });

        // Act
        var result = await _activity.Run(payload);

        // Assert — the exception from the upload task should be caught and returned as GeneralError
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Contains("Simulated upload failure", result.FailedItem.ErrorMessage);
    }

    [Fact]
    public async Task Run_MultipartUpload_CancellationDuringUpload_ThrowsOperationCanceledException()
    {
        // Arrange — cancel after first upload chunk starts
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("abcdefgh"); // 8 bytes = 2 parts
        var stream = new MemoryStream(content);
        var contentLength = content.Length;
        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };
        var cts = new CancellationTokenSource();

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
            .ReturnsAsync((UploadSession s, int partNum, byte[] data, long start, long end, long total, string token,
                string? bucket) =>
            {
                // Cancel after first chunk is processing
                cts.Cancel();
                return new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum);
            });

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _activity.Run(payload, cts.Token));
    }

    [Fact]
    public async Task Run_MultipartUpload_SingleChunkFile_CompletesSuccessfully()
    {
        // Arrange — file size exactly equals chunk size (4 bytes) = 1 part
        var payload = CreatePayload();
        var content = Encoding.UTF8.GetBytes("WXYZ");
        var stream = new MemoryStream(content);
        var contentLength = content.Length;
        var session = new UploadSession { UploadId = Guid.NewGuid().ToString() };
        var uploadedParts = new ConcurrentDictionary<int, (byte[] Data, long Start, long End)>();

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
            .ReturnsAsync((UploadSession s, int partNum, byte[] data, long start, long end, long total, string token,
                string? bucket) =>
            {
                uploadedParts[partNum] = (data, start, end);
                return new UploadChunkResult(TransferDirection.EgressToNetApp, $"etag{partNum}", partNum);
            });

        _destinationClientMock
            .Setup(x => x.CompleteUploadAsync(
                session,
                null,
                It.IsAny<Dictionary<int, string>>(),
                payload.BearerToken,
                payload.BucketName,
                payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(uploadedParts);

        Assert.True(uploadedParts.TryGetValue(1, out var p1));
        Assert.Equal(0, p1.Start);
        Assert.Equal(3, p1.End);
        Assert.Equal(Encoding.UTF8.GetBytes("WXYZ"), p1.Data);
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

    [Fact]
    public async Task Run_AmazonS3ExceptionWith500_ReturnsTransientErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("We encountered an internal error. Please try again.")
                { StatusCode = HttpStatusCode.InternalServerError });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Contains("HTTP 500", result.FailedItem.ErrorMessage);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith404_ReturnsTransientErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("The specified upload does not exist.")
                { StatusCode = HttpStatusCode.NotFound });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Contains("HTTP 404", result.FailedItem.ErrorMessage);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith403_ReturnsGeneralErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("Access Denied") { StatusCode = HttpStatusCode.Forbidden });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith403AndInvalidAccessKeyId_ReturnsTransientErrorCode()
    {
        // Credentials were rotated on NetApp between the S3 client being created and the
        // upload request completing - the error code uniquely identifies this case.
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = "InvalidAccessKeyId" });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith403AndExpiredToken_ReturnsTransientErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("The provided token has expired.")
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = "ExpiredToken" });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith403AndKnownMessageButNoErrorCode_ReturnsTransientErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                { StatusCode = HttpStatusCode.Forbidden });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_AmazonS3ExceptionWith403Generic_StillReturnsGeneralErrorCode()
    {
        // A plain Access Denied (e.g. bucket permission error) must NOT be treated as
        // transient - it is a permanent error and should not cause orchestrator retries.
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new AmazonS3Exception("Access Denied")
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = "AccessDenied" });

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.GeneralError, result.FailedItem.ErrorCode);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }

    [Fact]
    public async Task Run_HttpIOException_ReturnsTransientErrorCode()
    {
        var payload = CreatePayload();

        _storageClientFactoryMock
            .Setup(x => x.GetClientsForDirection(payload.TransferDirection))
            .Returns((_sourceClientMock.Object, _destinationClientMock.Object));

        _sourceClientMock
            .Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Net.Http.HttpIOException(System.Net.Http.HttpRequestError.ResponseEnded,
                "The response ended prematurely, with at least 6382474147 additional bytes expected."));

        var result = await _activity.Run(payload);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.FailedItem);
        Assert.Equal(TransferErrorCode.Transient, result.FailedItem.ErrorCode);
        Assert.Contains("Transient stream error", result.FailedItem.ErrorMessage);
        Assert.Equal(payload.SourcePath.FullFilePath, result.FailedItem.SourcePath);
    }
}
using System.Net;
using Amazon.S3;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class ValidateSourceFilesTests
{
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock = new();
    private readonly Mock<IStorageClient> _sourceClientMock = new();
    private readonly Mock<IInitializationHandler> _initializationHandlerMock = new();
    private readonly Mock<ILogger<ValidateSourceFiles>> _loggerMock = new();
    private readonly ValidateSourceFiles _activity;

    public ValidateSourceFilesTests()
    {
        _storageClientFactoryMock
            .Setup(f => f.GetClientsForDirection(It.IsAny<TransferDirection>()))
            .Returns((_sourceClientMock.Object, Mock.Of<IStorageClient>()));

        _activity = new ValidateSourceFiles(
            _storageClientFactoryMock.Object,
            _initializationHandlerMock.Object,
            _loggerMock.Object);
    }

    private static ValidateSourceFilesPayload CreatePayload(params TransferSourcePath[] sourcePaths) => new()
    {
        TransferDirection = TransferDirection.NetAppToEgress,
        SourcePaths = [.. sourcePaths],
        WorkspaceId = "ws-1",
        BearerToken = "token",
        BucketName = "bucket",
        CaseId = 42,
        UserName = "testuser",
        CorrelationId = Guid.NewGuid()
    };

    [Fact]
    public async Task Run_WithEmptySourcePaths_ReturnsEmptyResultAndDoesNotCallStorage()
    {
        var payload = CreatePayload();

        var result = await _activity.Run(payload);

        Assert.Empty(result.Available);
        Assert.Empty(result.Missing);
        Assert.Empty(result.Failed);
        _sourceClientMock.Verify(
            c => c.FileExistsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenFileExists_AddsToAvailable()
    {
        var source = new TransferSourcePath { Path = "/root/a.txt", FileId = "file-1" };
        var payload = CreatePayload(source);
        payload.TransferDirection = TransferDirection.EgressToNetApp;

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, source.FileId))
            .ReturnsAsync(true);

        var result = await _activity.Run(payload);

        Assert.Single(result.Available);
        Assert.Equal(source.Path, result.Available[0].Path);
        Assert.Empty(result.Missing);
        Assert.Empty(result.Failed);
        _storageClientFactoryMock.Verify(f => f.GetClientsForDirection(TransferDirection.EgressToNetApp), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFileDoesNotExist_AddsToMissing()
    {
        var source = new TransferSourcePath { Path = "/root/missing.txt" };
        var payload = CreatePayload(source);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, source.FileId))
            .ReturnsAsync(false);

        var result = await _activity.Run(payload);

        Assert.Empty(result.Available);
        Assert.Single(result.Missing);
        Assert.Equal(source.Path, result.Missing[0].Path);
        Assert.Empty(result.Failed);
    }

    [Fact]
    public async Task Run_WhenNotFoundException_AddsToMissing()
    {
        var source = new TransferSourcePath { Path = "/root/gone.txt" };
        var payload = CreatePayload(source);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, source.FileId))
            .ThrowsAsync(new HttpRequestException("not found", null, HttpStatusCode.NotFound));

        var result = await _activity.Run(payload);

        Assert.Empty(result.Available);
        Assert.Single(result.Missing);
        Assert.Empty(result.Failed);
    }

    [Fact]
    public async Task Run_WhenAccessDenied_AddsToFailedWithoutPollingClassification()
    {
        var source = new TransferSourcePath { Path = "/root/denied.txt" };
        var payload = CreatePayload(source);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, source.FileId))
            .ThrowsAsync(new HttpRequestException("forbidden", null, HttpStatusCode.Forbidden));

        var result = await _activity.Run(payload);

        Assert.Empty(result.Available);
        Assert.Empty(result.Missing);
        var failed = Assert.Single(result.Failed);
        Assert.Equal(source.Path, failed.SourcePath);
        Assert.Equal(TransferErrorCode.GeneralError, failed.ErrorCode);
    }

    [Fact]
    public async Task Run_WhenServerError_Rethrows()
    {
        var source = new TransferSourcePath { Path = "/root/a.txt" };
        var payload = CreatePayload(source);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, source.FileId))
            .ThrowsAsync(new HttpRequestException("boom", null, HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() => _activity.Run(payload));
    }

    [Fact]
    public async Task Run_MixedBatch_PartitionsAvailableMissingAndFailed()
    {
        var available = new TransferSourcePath { Path = "/root/ok.txt" };
        var missing = new TransferSourcePath { Path = "/root/missing.txt" };
        var denied = new TransferSourcePath { Path = "/root/denied.txt" };
        var payload = CreatePayload(available, missing, denied);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(available.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, available.FileId))
            .ReturnsAsync(true);
        _sourceClientMock
            .Setup(c => c.FileExistsAsync(missing.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, missing.FileId))
            .ReturnsAsync(false);
        _sourceClientMock
            .Setup(c => c.FileExistsAsync(denied.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, denied.FileId))
            .ThrowsAsync(new HttpRequestException("forbidden", null, HttpStatusCode.Forbidden));

        var result = await _activity.Run(payload);

        Assert.Equal(available.Path, Assert.Single(result.Available).Path);
        Assert.Equal(missing.Path, Assert.Single(result.Missing).Path);
        Assert.Equal(denied.Path, Assert.Single(result.Failed).SourcePath);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithPayloadContext()
    {
        var payload = CreatePayload(new TransferSourcePath { Path = "/root/a.txt" });
        _sourceClientMock
            .Setup(c => c.FileExistsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(true);

        await _activity.Run(payload);

        _initializationHandlerMock.Verify(h => h.Initialize(payload.UserName!, payload.CorrelationId, payload.CaseId), Times.Once);
    }

    [Fact]
    public async Task Run_NetAppToEgress_PassesPathAndFileIdToExistsCheck()
    {
        var source = new TransferSourcePath { Path = "/netapp/file.bin", FileId = null };
        var payload = CreatePayload(source);

        _sourceClientMock
            .Setup(c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, null))
            .ReturnsAsync(true);

        await _activity.Run(payload);

        _sourceClientMock.Verify(
            c => c.FileExistsAsync(source.Path, payload.WorkspaceId, payload.BearerToken, payload.BucketName, null),
            Times.Once);
    }

    [Theory]
    [InlineData(typeof(FileNotFoundException))]
    public void IsNotFound_RecognisesMissingFileExceptions(Type exceptionType)
    {
        var ex = (Exception)Activator.CreateInstance(exceptionType, "missing")!;
        Assert.True(ValidateSourceFiles.IsNotFound(ex));
    }

    [Fact]
    public void IsNotFound_RecognisesS3NotFound()
    {
        var s3 = new AmazonS3Exception("missing") { StatusCode = HttpStatusCode.NotFound };
        Assert.True(ValidateSourceFiles.IsNotFound(s3));
        Assert.False(ValidateSourceFiles.IsServerError(s3));
    }

    [Fact]
    public void IsServerError_RecognisesHttp500()
    {
        Assert.True(ValidateSourceFiles.IsServerError(new HttpRequestException("err", null, HttpStatusCode.BadGateway)));
        Assert.False(ValidateSourceFiles.IsServerError(new HttpRequestException("err", null, HttpStatusCode.Forbidden)));
    }
}

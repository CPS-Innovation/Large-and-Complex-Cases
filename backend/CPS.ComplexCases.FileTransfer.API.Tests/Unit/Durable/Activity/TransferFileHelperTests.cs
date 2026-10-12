using System.Net;
using System.Text;
using Amazon.S3;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Polly.CircuitBreaker;
using Polly.RateLimiting;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class TransferFileHelperTests
{
    private static TransferFilePayload CreatePayload(
        string path = "original.txt",
        string? modifiedPath = null,
        TransferDirection direction = TransferDirection.NetAppToEgress) =>
        new()
        {
            CaseId = 1,
            WorkspaceId = "ws",
            BearerToken = "token",
            BucketName = "bucket",
            DestinationPath = "dest",
            UserName = "user",
            TransferDirection = direction,
            SourcePath = new TransferSourcePath { Path = path, ModifiedPath = modifiedPath },
        };

    [Fact]
    public void ResolveSourceFilePath_UsesModifiedPathWhenPresent()
    {
        Assert.Equal("renamed.txt", TransferFile.ResolveSourceFilePath(CreatePayload(modifiedPath: "renamed.txt")));
    }

    [Fact]
    public void ResolveSourceFilePath_FallsBackToPathWhenModifiedMissing()
    {
        Assert.Equal("original.txt", TransferFile.ResolveSourceFilePath(CreatePayload()));
    }

    [Fact]
    public void MapExceptionToFailureResult_FileExists_MapsAndFlagsConflictTelemetry()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new FileExistsException("exists"),
            TransferDirection.EgressToNetApp,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.True(mapped.LogFileConflict);
        Assert.Equal(TransferErrorCode.FileExists, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_TimeoutCancel_MapsToGeneralError()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new OperationCanceledException("timeout"),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.GeneralError, mapped.ErrorCode);
        Assert.Contains("HTTP request timed out", mapped.DiagnosticMessage);
    }

    [Fact]
    public void MapExceptionToFailureResult_RealCancel_Rethrows()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new OperationCanceledException(),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: true);

        Assert.True(mapped.Rethrow);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    public void MapExceptionToFailureResult_NetAppToEgress_404Or409_IsTransient(HttpStatusCode status)
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new HttpRequestException("race", null, status),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_EgressToNetApp_404_IsSourceFileNotFound()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new HttpRequestException("missing", null, HttpStatusCode.NotFound),
            TransferDirection.EgressToNetApp,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.SourceFileNotFound, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_FileNotFoundException_IsSourceFileNotFound()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new FileNotFoundException("Object key not found"),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.SourceFileNotFound, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_S3CredentialExpired_IsTransient()
    {
        var s3 = new AmazonS3Exception("The AWS Access Key Id does not exist in our records")
        {
            StatusCode = HttpStatusCode.Forbidden,
            ErrorCode = "InvalidAccessKeyId"
        };

        var mapped = TransferFile.MapExceptionToFailureResult(
            s3, TransferDirection.EgressToNetApp, isCancellationRequested: false);

        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_BrokenCircuit_IsTransient()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new BrokenCircuitException("The circuit is now open and is not allowing calls."),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_RateLimiterRejected_IsTransient()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new RateLimiterRejectedException(),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
    }

    [Fact]
    public void MapExceptionToFailureResult_TransientStorageException_IsTransient()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new TransientStorageException("Chunk 3 upload for upload abc failed after 4 attempts."),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
        Assert.Contains("failed after 4 attempts", mapped.DiagnosticMessage);
    }

    [Fact]
    public void MapExceptionToFailureResult_ConnectionFailureWithoutStatusCode_IsTransient()
    {
        var mapped = TransferFile.MapExceptionToFailureResult(
            new HttpRequestException("The connection was closed unexpectedly."),
            TransferDirection.NetAppToEgress,
            isCancellationRequested: false);

        Assert.False(mapped.Rethrow);
        Assert.Equal(TransferErrorCode.Transient, mapped.ErrorCode);
        Assert.Contains("Transient connection failure", mapped.DiagnosticMessage);
    }

    [Fact]
    public async Task ReadExactPartAsync_FillsBuffer()
    {
        var bytes = Encoding.UTF8.GetBytes("abcdefghij");
        await using var stream = new MemoryStream(bytes);
        var buffer = new byte[10];

        var read = await TransferFile.ReadExactPartAsync(
            stream, buffer, targetPartSize: 10, bytesProcessed: 0, totalSize: 10, CancellationToken.None);

        Assert.Equal(10, read);
        Assert.Equal(bytes, buffer);
    }

    [Fact]
    public async Task ReadExactPartAsync_UnexpectedEof_Throws()
    {
        var bytes = Encoding.UTF8.GetBytes("abc");
        await using var stream = new MemoryStream(bytes);
        var buffer = new byte[10];

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransferFile.ReadExactPartAsync(
                stream, buffer, targetPartSize: 10, bytesProcessed: 0, totalSize: 10, CancellationToken.None));
    }

    [Fact]
    public void BuildMultipartCompletionFilePath_UsesDirectionRules()
    {
        Assert.Equal(
            "dest/file.txt",
            TransferFile.BuildMultipartCompletionFilePath(
                CreatePayload(path: "file.txt", direction: TransferDirection.EgressToNetApp),
                "source.txt"));
        Assert.Equal(
            "dest/source.txt",
            TransferFile.BuildMultipartCompletionFilePath(
                CreatePayload(direction: TransferDirection.NetAppToNetApp),
                "source.txt"));
        Assert.Null(TransferFile.BuildMultipartCompletionFilePath(
            CreatePayload(direction: TransferDirection.NetAppToEgress),
            "source.txt"));
    }

    // 5 MB chunks are what every environment is configured with, so a 6 MB file is a 5 MB head plus
    // a 1 MB tail, and an 11 MB file is two 5 MB parts plus a 1 MB tail. The tail wins the race to
    // Egress whenever it is dispatched before part 1 has landed, which is what loses the file. At
    // the configured concurrency of 2 that covers everything up to three parts (15 MB).
    [Theory]
    [InlineData(5 * 1024 * 1024, 1)]
    [InlineData(6 * 1024 * 1024, 1)]
    [InlineData(10 * 1024 * 1024, 1)]
    [InlineData(11 * 1024 * 1024, 1)]
    [InlineData(15 * 1024 * 1024, 1)]
    [InlineData(16 * 1024 * 1024, 2)]
    [InlineData(20 * 1024 * 1024, 2)]
    [InlineData(100 * 1024 * 1024, 2)]
    public void ResolvePartUploadConcurrency_SerialisesFilesWhoseTailCanOvertakeTheHead(long totalSize, int expected)
    {
        Assert.Equal(
            expected,
            TransferFile.ResolvePartUploadConcurrency(totalSize, 5 * 1024 * 1024, maxConcurrentPartUploads: 2));
    }

    // The serial threshold tracks the configured concurrency: at 4 concurrent parts the tail of a
    // five-part file still goes out alongside part 1, so that file has to be serialised too.
    [Theory]
    [InlineData(25 * 1024 * 1024, 1)]
    [InlineData(30 * 1024 * 1024, 4)]
    public void ResolvePartUploadConcurrency_ScalesThresholdWithConfiguredConcurrency(long totalSize, int expected)
    {
        Assert.Equal(
            expected,
            TransferFile.ResolvePartUploadConcurrency(totalSize, 5 * 1024 * 1024, maxConcurrentPartUploads: 4));
    }

    [Fact]
    public void ResolvePartUploadConcurrency_NeverReturnsLessThanOne()
    {
        Assert.Equal(1, TransferFile.ResolvePartUploadConcurrency(100, chunkSizeBytes: 0, maxConcurrentPartUploads: 0));
        Assert.Equal(1, TransferFile.ResolvePartUploadConcurrency(100, chunkSizeBytes: 10, maxConcurrentPartUploads: 0));
    }
}

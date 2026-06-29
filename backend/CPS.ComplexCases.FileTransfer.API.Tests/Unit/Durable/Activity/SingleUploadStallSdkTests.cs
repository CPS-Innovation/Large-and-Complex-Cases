using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

/// <summary>
/// Guards the assumption behind the single-PUT error classification: when the AWS SDK reads the
/// request body and the wrapped source stream stalls, the IdleTimeoutReadStream throws a TimeoutException from inside the SDK's read. 
/// TransferFile only classifies that stall as Transient (retryable) if the SDK lets the TimeoutException propagate unwrapped.
/// If a future SDK upgrade rewraps stream exceptions, it would fall through to the general catch as GeneralError and stop being retried.
/// </summary>
public class SingleUploadStallSdkTests
{
    [Fact]
    public async Task AwsSdkPutObject_WhenInputStreamStalls_PropagatesTimeoutExceptionUnwrapped()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = "https://127.0.0.1:9",
            ForcePathStyle = true,
            MaxErrorRetry = 0,
            HttpClientFactory = new BodyReadingHttpClientFactory(),
        };

        using var s3 = new AmazonS3Client(new BasicAWSCredentials("key", "secret"), config);

        const long contentLength = 10L;
        using var stalledStream = new StalledReadStream(contentLength);
        var wrappedStream = new IdleTimeoutReadStream(stalledStream, TimeSpan.FromSeconds(1), CancellationToken.None);

        var request = new PutObjectRequest
        {
            BucketName = "bucket",
            Key = "key",
            InputStream = wrappedStream,
            DisablePayloadSigning = true,
            Headers = { ContentLength = contentLength }
        };

        var startedAt = DateTime.UtcNow;
        var ex = await Assert.ThrowsAsync<TimeoutException>(() => s3.PutObjectAsync(request));
        var elapsed = DateTime.UtcNow - startedAt;

        Assert.Contains("Source stream read stalled", ex.Message);
        Assert.True(elapsed < TimeSpan.FromSeconds(30), $"Stalled PUT took {elapsed} to fail.");
    }

    // Custom factory + handler so the SDK pipeline runs fully in-memory (no network/TLS) while still
    // streaming the request body, forcing a read of the stalled InputStream inside PutObjectAsync.
    private sealed class BodyReadingHttpClientFactory : Amazon.Runtime.HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
            => new(new BodyReadingHandler());

        public override string GetConfigUniqueString(IClientConfig clientConfig) => "body-reading";
    }

    private sealed class BodyReadingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                using var sink = new MemoryStream();
                await request.Content.CopyToAsync(sink, cancellationToken);
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }

    // Reports a length but its ReadAsync never completes until the supplied token is cancelled,
    // simulating a silently dropped socket mid-upload.
    private sealed class StalledReadStream(long length) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position { get; set; }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => new(Task.Delay(Timeout.Infinite, cancellationToken).ContinueWith(_ => 0, cancellationToken));

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

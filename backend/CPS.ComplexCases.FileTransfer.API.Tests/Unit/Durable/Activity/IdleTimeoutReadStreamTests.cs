using CPS.ComplexCases.FileTransfer.API.Durable.Activity;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class IdleTimeoutReadStreamTests
{
    [Fact]
    public async Task ReadAsync_StalledInnerRead_ThrowsTimeoutExceptionQuickly()
    {
        using var inner = new StallingStream();
        using var stream = new IdleTimeoutReadStream(inner, TimeSpan.FromMilliseconds(200), CancellationToken.None);

        var buffer = new byte[8];
        var startedAt = DateTime.UtcNow;

        var ex = await Record.ExceptionAsync(async () =>
        {
            int read = await stream.ReadAsync(buffer.AsMemory());
            Assert.Fail($"Expected a timeout but read {read} bytes.");
        });

        Assert.IsType<TimeoutException>(ex);
        Assert.True(DateTime.UtcNow - startedAt < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Read_Synchronous_StalledInnerRead_ThrowsTimeoutException()
    {
        // The AWS SDK reads the upload body via the synchronous Read overload on the single-PUT path,
        // so it must also honour the idle timeout.
        using var inner = new StallingStream();
        using var stream = new IdleTimeoutReadStream(inner, TimeSpan.FromMilliseconds(200), CancellationToken.None);

        var buffer = new byte[8];

        var ex = Record.Exception(() =>
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            Assert.Fail($"Expected a timeout but read {read} bytes.");
        });

        Assert.IsType<TimeoutException>(ex);
    }

    [Fact]
    public async Task ReadAsync_ActivityTokenCancelled_ThrowsOperationCanceledNotTimeout()
    {
        using var inner = new StallingStream();
        using var activityCts = new CancellationTokenSource();
        using var stream = new IdleTimeoutReadStream(inner, TimeSpan.FromSeconds(30), activityCts.Token);

        var buffer = new byte[8];
        var readTask = stream.ReadAsync(buffer.AsMemory()).AsTask();
        await activityCts.CancelAsync();

        var ex = await Record.ExceptionAsync(async () =>
        {
            int read = await readTask;
            Assert.Fail($"Expected cancellation but read {read} bytes.");
        });

        Assert.IsType<TaskCanceledException>(ex);
    }

    [Fact]
    public async Task ReadAsync_HealthyRead_ReturnsBytesAndDoesNotTimeout()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        using var inner = new MemoryStream(payload);
        using var stream = new IdleTimeoutReadStream(inner, TimeSpan.FromSeconds(30), CancellationToken.None);

        var buffer = new byte[payload.Length];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory());

        Assert.Equal(payload.Length, bytesRead);
        Assert.Equal(payload, buffer);
    }

    // Inner stream whose reads never complete until the supplied token is cancelled.
    private sealed class StallingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => long.MaxValue;
        public override long Position { get; set; }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

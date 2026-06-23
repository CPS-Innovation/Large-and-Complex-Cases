namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Wraps a source stream and applies a per-read idle timeout linked to an activity cancellation token.
/// The storage HttpClient runs with an infinite timeout, so a streamed download body read is otherwise
/// only bounded by the 12h function timeout. This wrapper makes any read that fails to make progress
/// within <c>idleTimeout</c> throw a <see cref="TimeoutException"/>, regardless of which consumer reads
/// the stream (multipart loop or NetApp single PUT via the AWS SDK).
/// </summary>
public sealed class IdleTimeoutReadStream(Stream inner, TimeSpan idleTimeout, CancellationToken activityToken) : Stream
{
    private readonly Stream _inner = inner;
    private readonly TimeSpan _idleTimeout = idleTimeout;
    private readonly CancellationToken _activityToken = activityToken;

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => throw new NotSupportedException();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_activityToken, cancellationToken);
        cts.CancelAfter(_idleTimeout);

        try
        {
            return await _inner.ReadAsync(buffer, cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested
            && !_activityToken.IsCancellationRequested
            && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Source stream read stalled for {_idleTimeout.TotalSeconds:F0}s.");
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    // Synchronous reads (e.g. the AWS SDK reading the upload body) are routed through the async path
    // so they inherit the same idle timeout instead of blocking indefinitely.
    public override int Read(byte[] buffer, int offset, int count)
        => ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }
}

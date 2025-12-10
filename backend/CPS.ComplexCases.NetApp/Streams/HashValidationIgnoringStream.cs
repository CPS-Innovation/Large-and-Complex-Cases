using Amazon.Runtime;

namespace CPS.ComplexCases.NetApp.Streams;

/// <summary>
/// A stream wrapper that safely handles the AWS SDK's HashStream disposal.
/// When the inner S3 response stream is disposed, the AWS SDK may throw an
/// AmazonClientException if the expected hash doesn't match the calculated hash.
/// This wrapper catches and suppresses that specific exception during disposal.
/// </summary>
public class HashValidationIgnoringStream : Stream
{
    private readonly Stream _innerStream;
    private bool _disposed;

    public HashValidationIgnoringStream(Stream innerStream)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void Flush() => _innerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) =>
        _innerStream.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        _innerStream.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) =>
        _innerStream.Seek(offset, origin);

    public override void SetLength(long value) =>
        _innerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) =>
        _innerStream.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                _innerStream.Dispose();
            }
            catch (AmazonClientException ex) when (ex.Message.Contains("Expected hash not equal to calculated hash"))
            {
                // Suppress the hash validation exception that occurs when the AWS SDK's
                // HashStream is disposed. This happens when streaming from S3-compatible
                // storage (like NetApp) where the checksum validation may not align.
                // The data transfer has already completed successfully at this point.
            }
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await _innerStream.DisposeAsync();
        }
        catch (AmazonClientException ex) when (ex.Message.Contains("Expected hash not equal to calculated hash"))
        {
            // Suppress the hash validation exception (see Dispose comment above)
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
namespace CPS.ComplexCases.API.Integration.Tests.Helpers;

/// <summary>
/// Helper class for stream operations in tests.
/// </summary>
public static class StreamHelper
{
    /// <summary>
    /// Reads all bytes from a stream asynchronously.
    /// </summary>
    public static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Reads stream content as string.
    /// </summary>
    public static async Task<string> ReadAsStringAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Compares two streams for content equality.
    /// </summary>
    public static async Task<bool> AreStreamsEqualAsync(Stream stream1, Stream stream2)
    {
        var buffer1 = new byte[4096];
        var buffer2 = new byte[4096];

        while (true)
        {
            var count1 = await stream1.ReadAsync(buffer1);
            var count2 = await stream2.ReadAsync(buffer2);

            if (count1 != count2)
                return false;

            if (count1 == 0)
                return true;

            if (!buffer1.AsSpan(0, count1).SequenceEqual(buffer2.AsSpan(0, count2)))
                return false;
        }
    }

    /// <summary>
    /// Reads chunks from a stream.
    /// </summary>
    public static async IAsyncEnumerable<(byte[] Data, long Start, long End)> ReadChunksAsync(
        Stream stream,
        long totalSize,
        int chunkSize = 5 * 1024 * 1024)
    {
        var buffer = new byte[chunkSize];
        long position = 0;

        while (position < totalSize)
        {
            var bytesToRead = (int)Math.Min(chunkSize, totalSize - position);
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead));

            if (bytesRead == 0)
                break;

            var chunk = new byte[bytesRead];
            Array.Copy(buffer, chunk, bytesRead);

            var start = position;
            var end = position + bytesRead - 1;
            position += bytesRead;

            yield return (chunk, start, end);
        }
    }
}

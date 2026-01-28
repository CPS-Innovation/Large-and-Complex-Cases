using System.Security.Cryptography;
using System.Text;

namespace CPS.ComplexCases.API.Integration.Tests.Helpers;

/// <summary>
/// Helper class for generating test data.
/// </summary>
public static class TestDataHelper
{
    private static readonly Random Random = new();

    /// <summary>
    /// Generates a unique test file name with timestamp.
    /// </summary>
    public static string GenerateTestFileName(string extension = "txt")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = Random.Next(1000, 9999);
        return $"integration-test-{timestamp}-{randomSuffix}.{extension}";
    }

    /// <summary>
    /// Generates test content of specified size.
    /// </summary>
    public static byte[] GenerateTestContent(int sizeInBytes = 1024)
    {
        var content = new byte[sizeInBytes];
        Random.NextBytes(content);
        return content;
    }

    /// <summary>
    /// Generates test content as a string.
    /// </summary>
    public static string GenerateTestContentString(int approximateSizeInBytes = 1024)
    {
        var sb = new StringBuilder();
        var timestamp = DateTime.UtcNow.ToString("O");
        sb.AppendLine($"Integration Test File - Generated at {timestamp}");
        sb.AppendLine(new string('=', 50));

        while (sb.Length < approximateSizeInBytes)
        {
            sb.AppendLine($"Line {Random.Next(1, 10000)}: {Guid.NewGuid()}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Computes MD5 hash of content as Base64 string (required for Egress uploads).
    /// </summary>
    public static string ComputeMd5Hash(byte[] content)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(content);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Computes MD5 hash of a stream (for large files).
    /// </summary>
    public static async Task<string> ComputeMd5HashAsync(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(stream);
        stream.Position = 0; // Reset stream position
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Generates a unique folder path for test isolation.
    /// Note: Uses single-level nesting under basePath because the Egress API
    /// cannot create root-level folders (parent path cannot be empty).
    /// The basePath folder must already exist in the workspace.
    /// </summary>
    public static string GenerateTestFolderPath(string basePath = "integration-tests")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var runId = Guid.NewGuid().ToString("N")[..8];
        // Use single-level nesting to avoid folder creation issues
        // The basePath must already exist in the Egress workspace
        return $"{basePath}/{timestamp}{runId}";
    }

    /// <summary>
    /// Splits content into chunks of specified size.
    /// </summary>
    public static IEnumerable<byte[]> SplitIntoChunks(byte[] content, int chunkSize = 5 * 1024 * 1024)
    {
        for (int i = 0; i < content.Length; i += chunkSize)
        {
            var remaining = Math.Min(chunkSize, content.Length - i);
            var chunk = new byte[remaining];
            Array.Copy(content, i, chunk, 0, remaining);
            yield return chunk;
        }
    }
}

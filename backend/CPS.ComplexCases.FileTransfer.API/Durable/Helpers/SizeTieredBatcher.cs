using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

/// <summary>
/// Splits a transfer's files into fan-out batches sized by tier. Small files upload in a single
/// request and tolerate a wide fan-out, whereas each large file holds several concurrent chunk
/// requests open, so a batch of those has to be narrower to stay within what Egress will absorb.
/// </summary>
public static class SizeTieredBatcher
{
    public record Batch(TransferSourcePath[] Files, bool IsLargeFileTier);

    /// <summary>
    /// Returns the small-file batches followed by the large-file batches. Files with an unknown
    /// size are treated as small so directions that cannot report a size keep their existing
    /// throughput rather than being throttled by default.
    /// </summary>
    public static List<Batch> BuildBatches(
        IReadOnlyList<TransferSourcePath> files,
        long largeFileThresholdBytes,
        int smallFileBatchSize,
        int largeFileBatchSize)
    {
        var batches = new List<Batch>();

        var large = files.Where(f => f.FileSizeBytes > largeFileThresholdBytes).ToArray();
        var small = files.Where(f => !(f.FileSizeBytes > largeFileThresholdBytes)).ToArray();

        batches.AddRange(small.Chunk(Math.Max(1, smallFileBatchSize)).Select(c => new Batch(c, false)));
        batches.AddRange(large.Chunk(Math.Max(1, largeFileBatchSize)).Select(c => new Batch(c, true)));

        return batches;
    }
}

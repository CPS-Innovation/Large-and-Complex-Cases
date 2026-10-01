using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Helpers;

public class SizeTieredBatcherTests
{
    private const long Threshold = 5 * 1024 * 1024;

    private static TransferSourcePath File(string path, long? sizeBytes) =>
        new() { Path = path, FileSizeBytes = sizeBytes };

    [Fact]
    public void BuildBatches_SmallFiles_UseTheSmallFileBatchSize()
    {
        var files = Enumerable.Range(0, 25).Select(i => File($"small-{i}", 50 * 1024)).ToList();

        var batches = SizeTieredBatcher.BuildBatches(files, Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4);

        Assert.Equal([12, 12, 1], batches.Select(b => b.Files.Length));
        Assert.All(batches, b => Assert.False(b.IsLargeFileTier));
    }

    [Fact]
    public void BuildBatches_LargeFiles_UseTheLargeFileBatchSize()
    {
        var files = Enumerable.Range(0, 9).Select(i => File($"large-{i}", 20 * 1024 * 1024)).ToList();

        var batches = SizeTieredBatcher.BuildBatches(files, Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4);

        Assert.Equal([4, 4, 1], batches.Select(b => b.Files.Length));
        Assert.All(batches, b => Assert.True(b.IsLargeFileTier));
    }

    [Fact]
    public void BuildBatches_MixedSizes_KeepsTiersInSeparateBatchesWithSmallFilesFirst()
    {
        var files = new List<TransferSourcePath>
        {
            File("large-1", 20 * 1024 * 1024),
            File("small-1", 50 * 1024),
            File("large-2", 6 * 1024 * 1024),
            File("small-2", 50 * 1024),
        };

        var batches = SizeTieredBatcher.BuildBatches(files, Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4);

        Assert.Equal(2, batches.Count);
        Assert.Equal(["small-1", "small-2"], batches[0].Files.Select(f => f.Path));
        Assert.False(batches[0].IsLargeFileTier);
        Assert.Equal(["large-1", "large-2"], batches[1].Files.Select(f => f.Path));
        Assert.True(batches[1].IsLargeFileTier);
    }

    [Fact]
    public void BuildBatches_FileExactlyAtThreshold_IsTreatedAsSmall()
    {
        // The threshold is the single-upload cut-off, and a file at the limit still uploads in one
        // request, so it belongs in the wide tier.
        var batches = SizeTieredBatcher.BuildBatches(
            [File("boundary", Threshold)], Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4);

        Assert.False(Assert.Single(batches).IsLargeFileTier);
    }

    [Fact]
    public void BuildBatches_UnknownSize_IsTreatedAsSmallToPreserveThroughput()
    {
        var batches = SizeTieredBatcher.BuildBatches(
            [File("unknown", null)], Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4);

        Assert.False(Assert.Single(batches).IsLargeFileTier);
    }

    [Fact]
    public void BuildBatches_NoFiles_ReturnsNoBatches()
    {
        Assert.Empty(SizeTieredBatcher.BuildBatches([], Threshold, smallFileBatchSize: 12, largeFileBatchSize: 4));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BuildBatches_NonPositiveBatchSizes_FallBackToOneFilePerBatch(int batchSize)
    {
        var files = new List<TransferSourcePath> { File("small", 1024), File("large", 20 * 1024 * 1024) };

        var batches = SizeTieredBatcher.BuildBatches(files, Threshold, batchSize, batchSize);

        Assert.Equal(2, batches.Count);
        Assert.All(batches, b => Assert.Single(b.Files));
    }
}

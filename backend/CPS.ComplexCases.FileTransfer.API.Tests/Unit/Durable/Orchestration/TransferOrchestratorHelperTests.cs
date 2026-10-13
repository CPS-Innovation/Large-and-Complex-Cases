using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Orchestration;

public class TransferOrchestratorHelperTests
{
    [Fact]
    public void PartitionByDestinationCollision_SeparatesDuplicatesFromCleanFiles()
    {
        var sources = new List<TransferSourcePath>
        {
            new() { Path = "src/a.txt", RelativePath = "a.txt" },
            new() { Path = "src/b.txt", RelativePath = "b.txt" },
            new() { Path = "src/c.txt", RelativePath = "c.txt" },
        };
        var destinationFiles = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
        {
            ["dest/a.txt"] = 10,
            ["dest/c.txt"] = null,
        };

        var (duplicates, cleanFiles) = TransferOrchestrator.PartitionByDestinationCollision(
            sources, "dest/", sourceRootFolderPath: null, destinationFiles);

        // A file already at the destination is a conflict whether or not the listing reports a size.
        Assert.Equal(2, duplicates.Count);
        Assert.Equal(new[] { "src/a.txt", "src/c.txt" }, duplicates.Select(d => d.Source.Path));
        Assert.Equal(new[] { "dest/a.txt", "dest/c.txt" }, duplicates.Select(d => d.DestPath));
        Assert.Single(cleanFiles);
        Assert.Equal("src/b.txt", cleanFiles[0].Path);
    }

    [Fact]
    public void FindUnverifiedTransfers_WhenSizesMatch_VerifiesEverything()
    {
        var sources = new List<TransferSourcePath>
        {
            new() { Path = "src/a.txt", RelativePath = "a.txt" },
            new() { Path = "src/b.txt", RelativePath = "b.txt" },
        };
        var landedFiles = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
        {
            ["dest/a.txt"] = 100,
            ["dest/b.txt"] = 200,
        };
        var uploadedSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["src/a.txt"] = 100,
            ["src/b.txt"] = 200,
        };

        var unverified = TransferOrchestrator.FindUnverifiedTransfers(
            sources, "dest/", sourceRootFolderPath: null, landedFiles, uploadedSizes);

        Assert.Empty(unverified);
    }

    [Fact]
    public void FindUnverifiedTransfers_WhenFileIsAbsent_ReportsItAsMissing()
    {
        var sources = new List<TransferSourcePath> { new() { Path = "src/a.txt", RelativePath = "a.txt" } };
        var uploadedSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["src/a.txt"] = 100,
        };

        var unverified = TransferOrchestrator.FindUnverifiedTransfers(
            sources,
            "dest/",
            sourceRootFolderPath: null,
            new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase),
            uploadedSizes);

        var (source, destPath, reason) = Assert.Single(unverified);
        Assert.Equal("src/a.txt", source.Path);
        Assert.Equal("dest/a.txt", destPath);
        Assert.Equal(TransferOrchestrator.MissingFromDestinationMessage, reason);
    }

    // A failed Egress commit can leave a 0-byte or truncated file at the destination, which a
    // path-only check reads as a successful transfer.
    [Theory]
    [InlineData(0L)]
    [InlineData(40L)]
    [InlineData(200L)]
    public void FindUnverifiedTransfers_WhenLandedSizeDiffers_ReportsItAsIncomplete(long landedSize)
    {
        var sources = new List<TransferSourcePath> { new() { Path = "src/a.txt", RelativePath = "a.txt" } };
        var landedFiles = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
        {
            ["dest/a.txt"] = landedSize,
        };
        var uploadedSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["src/a.txt"] = 100,
        };

        var unverified = TransferOrchestrator.FindUnverifiedTransfers(
            sources, "dest/", sourceRootFolderPath: null, landedFiles, uploadedSizes);

        var (_, destPath, reason) = Assert.Single(unverified);
        Assert.Equal("dest/a.txt", destPath);
        Assert.Equal(TransferOrchestrator.IncompleteAtDestinationMessage, reason);
    }

    // Egress does not report a size for every listing entry, and an unknown size is not evidence
    // that the file is broken. Re-uploading on that basis would fail transfers that are fine.
    [Fact]
    public void FindUnverifiedTransfers_WhenDestinationReportsNoSize_FallsBackToThePathAlone()
    {
        var sources = new List<TransferSourcePath> { new() { Path = "src/a.txt", RelativePath = "a.txt" } };
        var landedFiles = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
        {
            ["dest/a.txt"] = null,
        };
        var uploadedSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["src/a.txt"] = 100,
        };

        var unverified = TransferOrchestrator.FindUnverifiedTransfers(
            sources, "dest/", sourceRootFolderPath: null, landedFiles, uploadedSizes);

        Assert.Empty(unverified);
    }

    [Fact]
    public void FindUnverifiedTransfers_MatchesTheDestinationListingCaseInsensitively()
    {
        var sources = new List<TransferSourcePath> { new() { Path = "src/a.txt", RelativePath = "A.txt" } };
        var landedFiles = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
        {
            ["dest/a.TXT"] = 100,
        };
        var uploadedSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["SRC/A.txt"] = 100,
        };

        var unverified = TransferOrchestrator.FindUnverifiedTransfers(
            sources, "dest/", sourceRootFolderPath: null, landedFiles, uploadedSizes);

        Assert.Empty(unverified);
    }

    [Fact]
    public void GetEgressDestinationPath_WhenRootPrefixMatches_StripsRoot()
    {
        var result = TransferOrchestrator.GetEgressDestinationPath(
            "dest/", "root/sub/file.txt", "root/");

        Assert.Equal("dest/sub/file.txt", result);
    }

    [Fact]
    public void GetEgressDestinationPath_WhenRootDoesNotMatch_AppendsRelativePath()
    {
        var result = TransferOrchestrator.GetEgressDestinationPath(
            "dest/", "other/file.txt", "root/");

        Assert.Equal("dest/other/file.txt", result);
    }
}

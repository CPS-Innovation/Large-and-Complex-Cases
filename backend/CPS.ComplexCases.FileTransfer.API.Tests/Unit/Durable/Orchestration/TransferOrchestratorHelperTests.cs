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
        var destinationFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dest/a.txt",
            "dest/c.txt",
        };

        var (duplicates, cleanFiles) = TransferOrchestrator.PartitionByDestinationCollision(
            sources, "dest/", sourceRootFolderPath: null, destinationFiles);

        Assert.Equal(2, duplicates.Count);
        Assert.Equal(new[] { "src/a.txt", "src/c.txt" }, duplicates.Select(d => d.Source.Path));
        Assert.Equal(new[] { "dest/a.txt", "dest/c.txt" }, duplicates.Select(d => d.DestPath));
        Assert.Single(cleanFiles);
        Assert.Equal("src/b.txt", cleanFiles[0].Path);
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

using AutoFixture;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class ListDestinationFilePathsTests
{
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock;
    private readonly Mock<IStorageClient> _egressStorageClientMock;
    private readonly ListDestinationFilePaths _activity;
    private readonly Fixture _fixture;
    private readonly string _workspaceId;

    public ListDestinationFilePathsTests()
    {
        _storageClientFactoryMock = new Mock<IStorageClientFactory>();
        _egressStorageClientMock = new Mock<IStorageClient>();
        _fixture = new Fixture();

        _workspaceId = _fixture.Create<string>();

        _storageClientFactoryMock
            .Setup(x => x.GetClient(StorageProvider.Egress))
            .Returns(_egressStorageClientMock.Object);

        _activity = new ListDestinationFilePaths(_storageClientFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ReturnsFilePathsWithTheirSizes_WhenFilesExist()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>
        {
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/file1.txt",
                FileSizeBytes = 100
            },
            new() {
                SourcePath = "path/file2.txt",
                FullFilePath = "dest/file2.txt",
                FileSizeBytes = 0
            },
            // Egress does not report a size for every listing entry.
            new() {
                SourcePath = "path/file3.txt",
                FullFilePath = "dest/file3.txt",
                FileSizeBytes = null
            }
        };

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(100, result["dest/file1.txt"]);
        Assert.Equal(0, result["dest/file2.txt"]);
        Assert.Null(result["dest/file3.txt"]);
    }

    [Fact]
    public async Task Run_FiltersOutNullFullFilePath_WhenFileHasNullFullFilePath()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>
        {
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/file1.txt"
            },
            new() {
                SourcePath = "path/file2.txt",
                FullFilePath = null
            },
            new() {
                SourcePath = "path/file3.txt",
                FullFilePath = "dest/file3.txt"
            }
        };

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(["dest/file1.txt", "dest/file3.txt"], result.Keys);
    }

    [Fact]
    public async Task Run_FiltersOutEmptyFullFilePath_WhenFileHasEmptyFullFilePath()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>
        {
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/file1.txt"
            },
            new() {
                SourcePath = "path/file2.txt",
                FullFilePath = ""
            },
            new() {
                SourcePath = "path/file3.txt",
                FullFilePath = "dest/file3.txt"
            }
        };

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(["dest/file1.txt", "dest/file3.txt"], result.Keys);
    }

    [Fact]
    public async Task Run_ReturnsEmptyListing_WhenNoFilesExist()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>();

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Run_ReturnsEmptyListing_WhenAllFilesHaveNullOrEmptyFullFilePath()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>
        {
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = null
            },
            new() {
                SourcePath = "path/file2.txt",
                FullFilePath = ""
            }
        };

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Run_UsesCaseInsensitiveComparison_WhenCheckingForDuplicates()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>
        {
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/File1.txt",
                FileSizeBytes = 100
            },
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/file1.txt",
                FileSizeBytes = 100
            },
            new() {
                SourcePath = "path/file1.txt",
                FullFilePath = "dest/FILE1.TXT",
                FileSizeBytes = 100
            }
        };

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        var result = await _activity.Run(payload);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100, result["dest/File1.txt"]);
        Assert.Equal(100, result["dest/file1.txt"]);
        Assert.Equal(100, result["dest/FILE1.TXT"]);
    }

    [Fact]
    public async Task Run_CallsGetAllFilesFromFolderAsync_WithEmptyFolderPathAndWorkspaceId()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");

        var files = new List<FileTransferInfo>();

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        await _activity.Run(payload);

        // Assert
        _egressStorageClientMock.Verify(
            x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId),
            Times.Once);
    }

    [Fact]
    public async Task Run_GetsEgressClient_FromStorageClientFactory()
    {
        // Arrange
        var payload = new ListDestinationPayload(_workspaceId, "dest/path");
        var files = new List<FileTransferInfo>();

        _egressStorageClientMock
            .Setup(x => x.GetAllFilesFromFolderAsync("dest/path", _workspaceId))
            .ReturnsAsync(files);

        // Act
        await _activity.Run(payload);

        // Assert
        _storageClientFactoryMock.Verify(
            x => x.GetClient(StorageProvider.Egress),
            Times.AtLeastOnce);
    }
}

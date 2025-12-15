using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.WireMock.Mappings;
using CPS.ComplexCases.WireMock.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.Tests.Integration;

public class EgressStorageClientTests : IDisposable
{
    private bool _disposed = false;
    private readonly WireMockServer _server;
    private readonly EgressStorageClient _client;
    private readonly HttpClient _httpClient;

    public EgressStorageClientTests()
    {
        _server = WireMockServer
            .Start()
            .LoadMappings(
                new CaseDocumentMapping(),
                new CreateUploadMapping(),
                new WorkspaceTokenMapping(),
                new CaseMaterialMapping()
            );

        var egressOptions = new EgressOptions
        {
            Url = _server.Urls[0],
            Username = "username",
            Password = "password"
        };

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(egressOptions.Url)
        };
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EgressStorageClient>();

        _client = new EgressStorageClient(logger, new OptionsWrapper<EgressOptions>(egressOptions), _httpClient, new EgressRequestFactory());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _server.Stop();
                _httpClient.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EgressStorageClientTests()
    {
        Dispose(false);
    }

    [Fact]
    public async Task OpenReadStreamAsync_ShouldReturnFileStream()
    {
        // Arrange
        const string workspaceId = "workspace-id";
        const string fileId = "file-id";
        const string path = "/test/file.txt";

        // Act
        var (stream, contentLength) = await _client.OpenReadStreamAsync(path, workspaceId, fileId);

        // Assert
        await using (stream)
        {
            Assert.NotNull(stream);
            Assert.True(stream.CanRead);

            // Verify we can read content from the stream
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            Assert.NotNull(content);
        }
    }

    [Fact]
    public async Task OpenReadStreamAsync_ShouldThrowArgumentNullException_WhenWorkspaceIdIsNull()
    {
        // Arrange
        const string fileId = "file-id";
        const string path = "/test/file.txt";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.OpenReadStreamAsync(path, null, fileId));

        Assert.Equal("workspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null", exception.Message);
    }

    [Fact]
    public async Task OpenReadStreamAsync_ShouldThrowArgumentNullException_WhenFileIdIsNull()
    {
        // Arrange
        const string workspaceId = "workspace-id";
        const string path = "/test/file.txt";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.OpenReadStreamAsync(path, workspaceId, null));

        Assert.Equal("fileId", exception.ParamName);
        Assert.Contains("File ID cannot be null", exception.Message);
    }
    [Fact]
    public async Task InitiateUploadAsync_ShouldReturnUploadSession()
    {
        // Arrange
        const string destinationPath = "/uploads/test";
        const long fileSize = 1024;
        const string workspaceId = "workspace-id";
        const string sourcePath = "/local/test-file.txt";
        const string relativePath = "test-file.txt";

        // Act
        var result = await _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, workspaceId, relativePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock-upload-id-12345", result.UploadId);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", result.Md5Hash);
    }

    [Fact]
    public async Task InitiateUploadAsync_ShouldThrowArgumentNullException_WhenWorkspaceIdIsNull()
    {
        // Arrange
        const string destinationPath = "/uploads/test";
        const long fileSize = 1024;
        const string sourcePath = "/local/test-file.txt";
        const string relativePath = "test-file.txt";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, null, relativePath));

        Assert.Equal("workspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null", exception.Message);
    }

    [Fact]
    public async Task InitiateUploadAsync_ShouldThrowArgumentNullException_WhenSourcePathIsNull()
    {
        // Arrange
        const string destinationPath = "/uploads/test";
        const long fileSize = 1024;
        const string workspaceId = "workspace-id";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.InitiateUploadAsync(destinationPath, fileSize, workspaceId, null, null));

        Assert.Equal("relativePath", exception.ParamName);
        Assert.Contains("Relative path cannot be null or empty.", exception.Message);
    }

    [Fact]
    public async Task UploadChunkAsync_ShouldReturnUploadChunkResult()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = "mock-upload-id-12345",
            WorkspaceId = "workspace-id",
            Md5Hash = "d41d8cd98f00b204e9800998ecf8427e"
        };
        const int chunkNumber = 1;
        var chunkData = new byte[] { 1, 2, 3, 4, 5 };
        const long start = 0;
        const long end = 4;
        const long totalSize = 5;

        // Act
        var result = await _client.UploadChunkAsync(session, chunkNumber, chunkData, start, end, totalSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransferDirection.NetAppToEgress, result.TransferDirection);
    }

    [Fact]
    public async Task UploadChunkAsync_ShouldThrowArgumentNullException_WhenWorkspaceIdIsNull()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = "mock-upload-id-12345",
            WorkspaceId = null
        };
        const int chunkNumber = 1;
        var chunkData = new byte[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.UploadChunkAsync(session, chunkNumber, chunkData));

        Assert.Equal("WorkspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null", exception.Message);
    }

    [Fact]
    public async Task CompleteUploadAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = "mock-upload-id-12345",
            WorkspaceId = "workspace-id",
            Md5Hash = "d41d8cd98f00b204e9800998ecf8427e"
        };
        const string md5Hash = "d41d8cd98f00b204e9800998ecf8427e";
        var etags = new Dictionary<int, string> { { 1, "etag1" }, { 2, "etag2" } };

        // Act & Assert (should not throw)
        await _client.CompleteUploadAsync(session, md5Hash, etags);
    }

    [Fact]
    public async Task CompleteUploadAsync_ShouldThrowArgumentNullException_WhenWorkspaceIdIsNull()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = "mock-upload-id-12345",
            WorkspaceId = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.CompleteUploadAsync(session));

        Assert.Equal("WorkspaceId", exception.ParamName);
        Assert.Contains("Workspace ID cannot be null", exception.Message);
    }

    [Fact]
    public async Task CompleteUploadAsync_ShouldCompleteWithoutOptionalParameters()
    {
        // Arrange
        var session = new UploadSession
        {
            UploadId = "mock-upload-id-12345",
            WorkspaceId = "workspace-id",
            Md5Hash = "d41d8cd98f00b204e9800998ecf8427e"
        };

        // Act & Assert (should not throw)
        await _client.CompleteUploadAsync(session);
    }

    [Fact]
    public async Task FullUploadWorkflow_ShouldCompleteSuccessfully()
    {
        // Arrange
        const string destinationPath = "/uploads/test";
        const long fileSize = 10;
        const string workspaceId = "workspace-id";
        const string sourcePath = "/local/test-file.txt";
        const string relativePath = "test-file.txt";
        var chunkData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        const long start = 0;
        const long end = 9;
        const long totalSize = 10;
        const string md5Hash = "d41d8cd98f00b204e9800998ecf8427e";

        // Act
        // Step 1: Initiate upload (will check for existing files and find none)
        var session = await _client.InitiateUploadAsync(destinationPath, fileSize, sourcePath, workspaceId, relativePath);
        Assert.NotNull(session);
        Assert.NotNull(session.UploadId);

        // Step 2: Upload chunk
        var chunkResult = await _client.UploadChunkAsync(session, 1, chunkData, start, end, totalSize);
        Assert.NotNull(chunkResult);
        Assert.Equal(TransferDirection.NetAppToEgress, chunkResult.TransferDirection);

        // Step 3: Complete upload
        await _client.CompleteUploadAsync(session, md5Hash);

        // Assert - No exceptions thrown indicates success
        Assert.True(true, "Full upload workflow completed successfully");
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithSingleFile_ShouldReturnFileInfo()
    {
        // Arrange
        const string workspaceId = "workspace-id";
        var selectedEntities = new List<TransferEntityDto>
    {
        new TransferEntityDto
        {
            Id = "file-id",
            FileId = "file-id",
            Path = "/test/file.txt",
            IsFolder = false
        }
    };

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        var files = result.ToList();
        Assert.Single(files);
        Assert.Equal("file-id", files[0].Id);
        Assert.Equal("file.txt", files[0].SourcePath);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithFolder_ShouldReturnFilesFromFolder()
    {
        // Arrange
        const string workspaceId = "workspace-id";
        var selectedEntities = new List<TransferEntityDto>
        {
        new TransferEntityDto
        {
            Id = "folder-id",
            Path = "/test/folder",
            IsFolder = true,
            FileId = "folder-id"
        }
        };

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        var files = result.ToList();
        Assert.Single(files);
        Assert.Equal("nested-file-id", files[0].Id);
        Assert.Equal("folder-id/file-path/nested-file-name", files[0].SourcePath);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WithMixedEntities_ShouldReturnAllFiles()
    {
        // Arrange
        const string workspaceId = "workspace-id";
        var selectedEntities = new List<TransferEntityDto>
        {
            new TransferEntityDto
            {
                Id = "file-id",
                FileId = "file-id",
                Path = "/test/standalone-file.txt",
                IsFolder = false
            },
            new TransferEntityDto
            {
                Id = "folder-id",
                FileId = "folder-id",
                Path = "/test/folder",
                IsFolder = true
            }
        };

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, workspaceId);

        // Assert
        Assert.NotNull(result);
        var files = result.ToList();
        Assert.Equal(2, files.Count);

        Assert.Contains(files, f => f.Id == "file-id" && f.SourcePath == "standalone-file.txt");
        Assert.Contains(files, f => f.Id == "nested-file-id" && f.SourcePath == "folder-id/file-path/nested-file-name");
    }
    [Fact]
    public async Task UploadLargeFile_ShouldHandleMultipleChunks()
    {
        // Arrange
        const int largeFileSize = 50 * 1024 * 1024; // 50 MB
        var largeData = new byte[largeFileSize];
        new Random().NextBytes(largeData);

        var session = await _client.InitiateUploadAsync("/uploads/test", largeFileSize, "/local/large-file.txt", "workspace-id", "large-file.txt");
        int chunkSize = 5 * 1024 * 1024; // 5 MB
        int totalChunks = (int)Math.Ceiling((double)largeFileSize / chunkSize);

        // Act & Assert
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * chunkSize;
            int size = Math.Min(chunkSize, largeFileSize - offset);
            var chunk = new byte[size];
            Array.Copy(largeData, offset, chunk, 0, size);
            var start = (long)offset;
            var end = (long)(offset + size - 1);
            var totalSize = (long)largeFileSize;
            var result = await _client.UploadChunkAsync(session, i + 1, chunk, start, end, totalSize);
            Assert.NotNull(result);
        }

        await _client.CompleteUploadAsync(session, "dummy-md5");
    }
}
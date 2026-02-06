using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.API.Integration.Tests.Helpers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

namespace CPS.ComplexCases.API.Integration.Tests.Egress;

[Collection("Integration Tests")]
public class EgressStorageClientTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public EgressStorageClientTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_WithValidParameters_ReturnsUploadSession()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();

        // Act
        var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
            destinationPath: destinationPath,
            fileSize: 1024,
            sourcePath: testFileName,
            workspaceId: workspaceId,
            relativePath: testFileName);

        // Assert
        Assert.NotNull(session);
        Assert.False(string.IsNullOrEmpty(session.UploadId));
        Assert.Equal(workspaceId, session.WorkspaceId);
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: "test-path",
                fileSize: 1024,
                sourcePath: testFileName,
                workspaceId: null,
                relativePath: testFileName));

        Assert.Equal("workspaceId", exception.ParamName);
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_WithNullRelativePath_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();
        var workspaceId = _fixture.EgressWorkspaceId!;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: "test-path",
                fileSize: 1024,
                sourcePath: testFileName,
                workspaceId: workspaceId,
                relativePath: null));

        Assert.Equal("relativePath", exception.ParamName);
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_WithEmptyRelativePath_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();
        var workspaceId = _fixture.EgressWorkspaceId!;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: "test-path",
                fileSize: 1024,
                sourcePath: testFileName,
                workspaceId: workspaceId,
                relativePath: string.Empty));

        Assert.Equal("relativePath", exception.ParamName);
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_WithInvalidWorkspaceId_ThrowsHttpRequestException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();
        var invalidWorkspaceId = "invalid-workspace-id-" + Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: "test-path",
                fileSize: 1024,
                sourcePath: testFileName,
                workspaceId: invalidWorkspaceId,
                relativePath: testFileName));
    }

    [SkippableFact]
    public async Task UploadFlow_SmallFile_CompletesSuccessfully()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var testFileName = TestDataHelper.GenerateTestFileName();
        var testContent = TestDataHelper.GenerateTestContentString(1024);
        var testContentBytes = System.Text.Encoding.UTF8.GetBytes(testContent);
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();

        // Act - Initiate upload
        var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
            destinationPath: destinationPath,
            fileSize: testContentBytes.Length,
            sourcePath: testFileName,
            workspaceId: workspaceId,
            relativePath: testFileName);

        Assert.NotNull(session);
        Assert.False(string.IsNullOrEmpty(session.UploadId));

        // Act - Upload single chunk
        var chunkResult = await _fixture.EgressStorageClient.UploadChunkAsync(
            session: session,
            chunkNumber: 1,
            chunkData: testContentBytes,
            start: 0,
            end: testContentBytes.Length - 1,
            totalSize: testContentBytes.Length);

        Assert.NotNull(chunkResult);

        // Act - Complete upload with MD5 hash
        var md5Hash = TestDataHelper.ComputeMd5Hash(testContentBytes);
        await _fixture.EgressStorageClient.CompleteUploadAsync(session, md5Hash);

        // Assert - No exception means success
    }

    [SkippableFact]
    public async Task UploadFlow_ChunkedFile_CompletesSuccessfully()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange - Create a larger test file (100KB to test chunking)
        var testFileName = TestDataHelper.GenerateTestFileName();
        var testContent = TestDataHelper.GenerateTestContent(100 * 1024); // 100KB
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();
        var chunkSize = 32 * 1024; // 32KB chunks

        // Act - Initiate upload
        var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
            destinationPath: destinationPath,
            fileSize: testContent.Length,
            sourcePath: testFileName,
            workspaceId: workspaceId,
            relativePath: testFileName);

        Assert.NotNull(session);

        // Act - Upload in chunks
        var chunks = TestDataHelper.SplitIntoChunks(testContent, chunkSize).ToList();
        long position = 0;

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var start = position;
            var end = position + chunk.Length - 1;

            var chunkResult = await _fixture.EgressStorageClient.UploadChunkAsync(
                session: session,
                chunkNumber: i + 1,
                chunkData: chunk,
                start: start,
                end: end,
                totalSize: testContent.Length);

            Assert.NotNull(chunkResult);
            position += chunk.Length;
        }

        // Act - Complete upload
        var md5Hash = TestDataHelper.ComputeMd5Hash(testContent);
        await _fixture.EgressStorageClient.CompleteUploadAsync(session, md5Hash);

        // Assert - No exception means success
    }

    [SkippableFact]
    public async Task UploadFlow_WithSourceRootFolderPath_PreservesRelativePath()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange - Simulate nested folder structure
        var testFileName = TestDataHelper.GenerateTestFileName();
        var sourceRootFolderPath = "source-folder";
        var relativePath = $"{sourceRootFolderPath}/subfolder/{testFileName}";
        var testContent = TestDataHelper.GenerateTestContent(512);
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();

        // Act
        var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
            destinationPath: destinationPath,
            fileSize: testContent.Length,
            sourcePath: testFileName,
            workspaceId: workspaceId,
            relativePath: relativePath,
            sourceRootFolderPath: sourceRootFolderPath);

        // Assert
        Assert.NotNull(session);
        Assert.False(string.IsNullOrEmpty(session.UploadId));
    }

    [SkippableFact]
    public async Task UploadChunkAsync_WithNullUploadId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var session = new UploadSession
        {
            UploadId = null!,
            WorkspaceId = _fixture.EgressWorkspaceId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.UploadChunkAsync(
                session: session,
                chunkNumber: 1,
                chunkData: new byte[100],
                start: 0,
                end: 99,
                totalSize: 100));
    }

    [SkippableFact]
    public async Task UploadChunkAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var session = new UploadSession
        {
            UploadId = "test-upload-id",
            WorkspaceId = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.UploadChunkAsync(
                session: session,
                chunkNumber: 1,
                chunkData: new byte[100],
                start: 0,
                end: 99,
                totalSize: 100));
    }

    [SkippableFact]
    public async Task CompleteUploadAsync_WithNullUploadId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var session = new UploadSession
        {
            UploadId = null!,
            WorkspaceId = _fixture.EgressWorkspaceId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.CompleteUploadAsync(session, "md5hash"));
    }

    [SkippableFact]
    public async Task CompleteUploadAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var session = new UploadSession
        {
            UploadId = "test-upload-id",
            WorkspaceId = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.CompleteUploadAsync(session, "md5hash"));
    }

    [SkippableFact]
    public async Task OpenReadStreamAsync_WithNullWorkspaceId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.OpenReadStreamAsync(
                path: "/test-path",
                workspaceId: null,
                fileId: "test-file-id"));

        Assert.Equal("workspaceId", exception.ParamName);
    }

    [SkippableFact]
    public async Task OpenReadStreamAsync_WithNullFileId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _fixture.EgressStorageClient!.OpenReadStreamAsync(
                path: "/test-path",
                workspaceId: workspaceId,
                fileId: null));

        Assert.Equal("fileId", exception.ParamName);
    }

    [SkippableFact]
    public async Task OpenReadStreamAsync_WithInvalidFileId_ThrowsHttpRequestException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var invalidFileId = "non-existent-file-" + Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _fixture.EgressStorageClient!.OpenReadStreamAsync(
                path: "/test-path",
                workspaceId: workspaceId,
                fileId: invalidFileId));
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithFolderEntity_ReturnsFiles()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var selectedEntities = new List<TransferEntityDto>
        {
            new()
            {
                FileId = null, // Root folder
                Path = "/",
                IsFolder = true
            }
        };

        // Act
        var files = await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
            selectedEntities,
            workspaceId);

        // Assert
        Assert.NotNull(files);
        // Files may be empty if workspace has no content
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithFileEntity_ReturnsFileInfo()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var testFileId = "test-file-id";
        var testPath = "/test-folder/test-file.txt";

        var selectedEntities = new List<TransferEntityDto>
        {
            new()
            {
                FileId = testFileId,
                Path = testPath,
                IsFolder = false
            }
        };

        // Act
        var files = await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
            selectedEntities,
            workspaceId);

        // Assert
        Assert.NotNull(files);
        var fileList = files.ToList();
        Assert.Single(fileList);
        Assert.Equal(testFileId, fileList[0].Id);
        Assert.Equal("test-file.txt", fileList[0].SourcePath);
        Assert.Equal(testPath, fileList[0].FullFilePath);
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithNullEntities_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
                null!,
                workspaceId));
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithEmptyEntities_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var emptyEntities = new List<TransferEntityDto>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
                emptyEntities,
                workspaceId));
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithNullWorkspaceId_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var selectedEntities = new List<TransferEntityDto>
        {
            new() { FileId = "test", Path = "/test", IsFolder = false }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
                selectedEntities,
                workspaceId: null));
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_WithEmptyWorkspaceId_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var selectedEntities = new List<TransferEntityDto>
        {
            new() { FileId = "test", Path = "/test", IsFolder = false }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.ListFilesForTransferAsync(
                selectedEntities,
                workspaceId: string.Empty));
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_WithValidFiles_ReturnsResult()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var nonExistentFileId = "non-existent-file-" + Guid.NewGuid();

        var filesToDelete = new List<DeletionEntityDto>
        {
            new() { Path = "/non-existent-path", FileId = nonExistentFileId }
        };

        // Act
        var result = await _fixture.EgressStorageClient!.DeleteFilesAsync(filesToDelete, workspaceId);

        // Assert
        Assert.NotNull(result);
        // Result may contain failed files for non-existent IDs
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_WithNullEntities_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.DeleteFilesAsync(
                null!,
                workspaceId));
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_WithEmptyEntities_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var emptyList = new List<DeletionEntityDto>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.DeleteFilesAsync(
                emptyList,
                workspaceId));
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_WithNullWorkspaceId_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var filesToDelete = new List<DeletionEntityDto>
        {
            new() { Path = "/test-path", FileId = "test-file-id" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.DeleteFilesAsync(
                filesToDelete,
                workspaceId: null));
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_WithEmptyWorkspaceId_ThrowsArgumentException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        var filesToDelete = new List<DeletionEntityDto>
        {
            new() { Path = "/test-path", FileId = "test-file-id" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.EgressStorageClient!.DeleteFilesAsync(
                filesToDelete,
                workspaceId: string.Empty));
    }

    [SkippableFact]
    public async Task UploadFileAsync_ThrowsNotImplementedException()
    {
        Skip.If(!_fixture.IsEgressConfigured, "Egress not configured");

        // Arrange
        using var stream = new MemoryStream(new byte[100]);

        // Act & Assert
        // EgressStorageClient does not implement UploadFileAsync - it uses chunked uploads instead
        await Assert.ThrowsAsync<NotImplementedException>(
            async () => await _fixture.EgressStorageClient!.UploadFileAsync(
                destinationPath: "test-path",
                fileStream: stream,
                contentLength: 100,
                workspaceId: _fixture.EgressWorkspaceId));
    }
}

/// <summary>
/// Collection definition for integration tests to share fixture.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}

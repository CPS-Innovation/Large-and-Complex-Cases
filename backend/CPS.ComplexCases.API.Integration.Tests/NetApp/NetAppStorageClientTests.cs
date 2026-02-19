using System.Text;
using Moq;
using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.API.Integration.Tests.Helpers;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.Common.Models.Domain;

namespace CPS.ComplexCases.API.Integration.Tests.NetApp;

[Collection("Integration Tests")]
public class NetAppStorageClientTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly string _testFolderPrefix;
    private NetAppStorageClient? _storageClient;
    private Mock<ICaseMetadataService>? _caseMetadataServiceMock;

    public NetAppStorageClientTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        var basePrefix = _fixture.NetAppTestFolderPrefix ?? $"integration-tests/{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _testFolderPrefix = $"{basePrefix}/storage-client-{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        if (!_fixture.IsNetAppConfigured)
            return;

        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();

        _storageClient = new NetAppStorageClient(
            _fixture.NetAppClient!,
            _fixture.NetAppArgFactory!,
            _caseMetadataServiceMock.Object,
            _fixture.NetAppS3HttpClient!,
            _fixture.NetAppS3HttpArgFactory!);

        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();

        var createFolderArg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            _testFolderPrefix);

        await _fixture.NetAppClient!.CreateFolderAsync(createFolderArg);
    }

    public async Task DisposeAsync()
    {
        if (!_fixture.IsNetAppConfigured)
            return;

        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();

        var deleteArg = _fixture.NetAppArgFactory!.CreateDeleteFileOrFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            "integration-test-cleanup",
            _testFolderPrefix);

        await _fixture.NetAppClient!.DeleteFileOrFolderAsync(deleteArg);
    }

    [SkippableFact]
    public async Task UploadFileAsync_SmallFile_UploadsSuccessfully()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testContent = $"Storage client integration test content - {DateTime.UtcNow:O}";
        var contentBytes = Encoding.UTF8.GetBytes(testContent);
        using var stream = new MemoryStream(contentBytes);
        var relativePath = $"upload-test-{Guid.NewGuid():N}.txt";

        // Act
        await _storageClient!.UploadFileAsync(
            destinationPath: _testFolderPrefix,
            fileStream: stream,
            contentLength: contentBytes.Length,
            relativePath: relativePath,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        var objectKey = $"{_testFolderPrefix}/{relativePath}";
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, objectKey);
        Assert.True(exists, "Uploaded file should exist");
    }

    [SkippableFact]
    public async Task UploadFileAsync_WithNestedPath_UploadsSuccessfully()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testContent = "Nested path upload test";
        var contentBytes = Encoding.UTF8.GetBytes(testContent);
        using var stream = new MemoryStream(contentBytes);
        var relativePath = $"nested/subfolder/file-{Guid.NewGuid():N}.txt";

        // Act
        await _storageClient!.UploadFileAsync(
            destinationPath: _testFolderPrefix,
            fileStream: stream,
            contentLength: contentBytes.Length,
            relativePath: relativePath,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        var objectKey = $"{_testFolderPrefix}/{relativePath}";
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, objectKey);
        Assert.True(exists, "Nested file should exist after upload");
    }

    [SkippableFact]
    public async Task UploadFileAsync_NullBearerToken_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.UploadFileAsync(
                destinationPath: _testFolderPrefix,
                fileStream: stream,
                contentLength: 4,
                relativePath: "test.txt",
                bearerToken: null,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task UploadFileAsync_NullBucketName_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.UploadFileAsync(
                destinationPath: _testFolderPrefix,
                fileStream: stream,
                contentLength: 4,
                relativePath: "test.txt",
                bearerToken: bearerToken,
                bucketName: null));
    }

    [SkippableFact]
    public async Task OpenReadStreamAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var nonExistentPath = $"{_testFolderPrefix}/non-existent-{Guid.NewGuid():N}.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _storageClient!.OpenReadStreamAsync(
                path: nonExistentPath,
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task OpenReadStreamAsync_NullBearerToken_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.OpenReadStreamAsync(
                path: "some/path.txt",
                bearerToken: null,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task MultipartUpload_FullWorkflow_CompletesSuccessfully()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var partSize = 5 * 1024 * 1024; // 5MB minimum part size for S3
        var part1Data = new byte[partSize];
        var part2Data = new byte[partSize];
        new Random().NextBytes(part1Data);
        new Random().NextBytes(part2Data);

        var sourcePath = $"multipart-storage-{Guid.NewGuid():N}.bin";
        var totalSize = part1Data.Length + part2Data.Length;

        // Act - Step 1: Initiate upload
        var session = await _storageClient!.InitiateUploadAsync(
            destinationPath: _testFolderPrefix,
            fileSize: totalSize,
            sourcePath: sourcePath,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        Assert.NotNull(session);
        Assert.NotEmpty(session.UploadId);
        Assert.NotNull(session.WorkspaceId);

        // Act - Step 2: Upload chunks
        var etags = new Dictionary<int, string>();

        var chunk1Result = await _storageClient!.UploadChunkAsync(
            session: session,
            chunkNumber: 1,
            chunkData: part1Data,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        Assert.NotNull(chunk1Result);
        Assert.NotNull(chunk1Result.ETag);
        etags[1] = chunk1Result.ETag;

        var chunk2Result = await _storageClient!.UploadChunkAsync(
            session: session,
            chunkNumber: 2,
            chunkData: part2Data,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        Assert.NotNull(chunk2Result);
        Assert.NotNull(chunk2Result.ETag);
        etags[2] = chunk2Result.ETag;

        await Task.Delay(TimeSpan.FromSeconds(5));

        // Act - Step 3: Complete upload
        await _storageClient!.CompleteUploadAsync(
            session: session,
            etags: etags,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!,
            filePath: session.WorkspaceId);

        // Assert
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, session.WorkspaceId);
        Assert.True(exists, "Multipart uploaded file should exist");
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_ValidParameters_ReturnsUploadSession()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var sourcePath = $"initiate-test-{Guid.NewGuid():N}.txt";

        // Act
        var session = await _storageClient!.InitiateUploadAsync(
            destinationPath: _testFolderPrefix,
            fileSize: 1024,
            sourcePath: sourcePath,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        Assert.NotNull(session);
        Assert.NotEmpty(session.UploadId);
        Assert.NotNull(session.WorkspaceId);
        Assert.Contains(sourcePath, session.WorkspaceId);
    }

    [SkippableFact]
    public async Task InitiateUploadAsync_NullBearerToken_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.InitiateUploadAsync(
                destinationPath: _testFolderPrefix,
                fileSize: 1024,
                sourcePath: "test.txt",
                bearerToken: null,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task CompleteUploadAsync_NullUploadId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var session = new Common.Models.Domain.UploadSession
        {
            UploadId = null!,
            WorkspaceId = "test-key"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.CompleteUploadAsync(
                session: session,
                etags: new Dictionary<int, string> { { 1, "etag" } },
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task ListFilesInFolder_FolderWithFiles_ReturnsFileList()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange - upload some test files
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testFolderPath = $"{_testFolderPrefix}/list-folder-{Guid.NewGuid():N}";

        // Upload a few test files
        for (int i = 1; i <= 3; i++)
        {
            var content = Encoding.UTF8.GetBytes($"Test file {i} content");
            using var stream = new MemoryStream(content);
            var objectKey = $"{testFolderPath}/file{i}.txt";

            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                objectKey,
                stream,
                content.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
        }

        // Act - Wait for files to be available
        IEnumerable<FileTransferInfo>? files = null;
        var found = await NetAppTestHelper.WaitUntilAsync(async () =>
        {
            files = await _storageClient!.ListFilesInFolder(
                path: testFolderPath,
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!);
            return files?.Count() == 3;
        });

        // Assert
        Assert.True(found, "Expected 3 files to be found");
        Assert.NotNull(files);
        var fileList = files.ToList();
        Assert.Equal(3, fileList.Count);
        Assert.All(fileList, f => Assert.Contains(testFolderPath, f.SourcePath));
    }

    [SkippableFact]
    public async Task ListFilesInFolder_EmptyFolder_ReturnsEmptyList()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var emptyFolderPath = $"{_testFolderPrefix}/empty-folder-{Guid.NewGuid():N}";

        // Create empty folder
        var createFolderArg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            emptyFolderPath);
        await _fixture.NetAppClient!.CreateFolderAsync(createFolderArg);

        // Act
        var files = await _storageClient!.ListFilesInFolder(
            path: emptyFolderPath,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        Assert.NotNull(files);
        Assert.Empty(files);
    }

    [SkippableFact]
    public async Task ListFilesInFolder_NestedFolders_ReturnsAllFilesRecursively()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var rootFolderPath = $"{_testFolderPrefix}/recursive-{Guid.NewGuid():N}";

        // Upload files at different levels
        var filePaths = new[]
        {
            $"{rootFolderPath}/root-file.txt",
            $"{rootFolderPath}/level1/level1-file.txt",
            $"{rootFolderPath}/level1/level2/level2-file.txt"
        };

        foreach (var filePath in filePaths)
        {
            var content = Encoding.UTF8.GetBytes($"Content for {filePath}");
            using var stream = new MemoryStream(content);

            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                filePath,
                stream,
                content.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
        }

        // Act - Wait for files to be available
        IEnumerable<FileTransferInfo>? files = null;
        var found = await NetAppTestHelper.WaitUntilAsync(async () =>
        {
            files = await _storageClient!.ListFilesInFolder(
                path: rootFolderPath,
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!);
            return files?.Count() == 3;
        });

        // Assert
        Assert.True(found, "Expected 3 files to be found recursively");
        Assert.NotNull(files);
        var fileList = files.ToList();
        Assert.Equal(3, fileList.Count);
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_SingleFile_ReturnsFileInfo()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testCaseId = 12345;
        var netAppFolderPath = _testFolderPrefix;

        // Upload a test file
        var filePath = $"{netAppFolderPath}/transfer-test-{Guid.NewGuid():N}.txt";
        var content = Encoding.UTF8.GetBytes("Transfer test content");
        using var stream = new MemoryStream(content);

        var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            filePath,
            stream,
            content.Length);
        await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);

        // Setup mock
        _caseMetadataServiceMock!.Setup(x => x.GetCaseMetadataForCaseIdAsync(testCaseId))
            .ReturnsAsync(new CaseMetadata
            {
                CaseId = testCaseId,
                NetappFolderPath = netAppFolderPath
            });

        var selectedEntities = new List<TransferEntityDto>
        {
            new() { Path = filePath }
        };

        // Act
        var result = await _storageClient!.ListFilesForTransferAsync(
            selectedEntities: selectedEntities,
            caseId: testCaseId,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        Assert.NotNull(result);
        var fileList = result.ToList();
        Assert.Single(fileList);
        Assert.Equal(filePath, fileList[0].SourcePath);
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_Folder_ReturnsAllFilesInFolder()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testCaseId = 12346;
        var netAppFolderPath = _testFolderPrefix;
        var testFolderPath = $"{netAppFolderPath}/transfer-folder-{Guid.NewGuid():N}";

        // Upload files to the folder
        for (int i = 1; i <= 2; i++)
        {
            var content = Encoding.UTF8.GetBytes($"File {i} for transfer");
            using var stream = new MemoryStream(content);
            var objectKey = $"{testFolderPath}/file{i}.txt";

            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                objectKey,
                stream,
                content.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
        }

        // Wait for files to be available
        await NetAppTestHelper.WaitUntilAsync(async () =>
        {
            var listArg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                prefix: testFolderPath,
                maxKeys: 10);
            var result = await _fixture.NetAppClient!.ListObjectsInBucketAsync(listArg);
            return result?.Data?.FileData?.Count() == 2;
        });

        // Setup mock
        _caseMetadataServiceMock!.Setup(x => x.GetCaseMetadataForCaseIdAsync(testCaseId))
            .ReturnsAsync(new CaseMetadata
            {
                CaseId = testCaseId,
                NetappFolderPath = netAppFolderPath
            });

        var selectedEntities = new List<TransferEntityDto>
        {
            new() { Path = testFolderPath } // Selecting a folder, not a file
        };

        // Act
        var result = await _storageClient!.ListFilesForTransferAsync(
            selectedEntities: selectedEntities,
            caseId: testCaseId,
            bearerToken: bearerToken,
            bucketName: _fixture.NetAppBucketName!);

        // Assert
        Assert.NotNull(result);
        var fileList = result.ToList();
        Assert.Equal(2, fileList.Count);
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_NullCaseId_ThrowsArgumentNullException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var selectedEntities = new List<TransferEntityDto> { new() { Path = "test.txt" } };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _storageClient!.ListFilesForTransferAsync(
                selectedEntities: selectedEntities,
                caseId: null,
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task ListFilesForTransferAsync_CaseMetadataNotFound_ThrowsInvalidOperationException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testCaseId = 99999;

        _caseMetadataServiceMock!.Setup(x => x.GetCaseMetadataForCaseIdAsync(testCaseId))
            .ReturnsAsync((CaseMetadata?)null);

        var selectedEntities = new List<TransferEntityDto> { new() { Path = "test.txt" } };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _storageClient!.ListFilesForTransferAsync(
                selectedEntities: selectedEntities,
                caseId: testCaseId,
                bearerToken: bearerToken,
                bucketName: _fixture.NetAppBucketName!));
    }

    [SkippableFact]
    public async Task DeleteFilesAsync_ThrowsNotImplementedException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var filesToDelete = new List<DeletionEntityDto>();

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(async () =>
            await _storageClient!.DeleteFilesAsync(filesToDelete));
    }
}
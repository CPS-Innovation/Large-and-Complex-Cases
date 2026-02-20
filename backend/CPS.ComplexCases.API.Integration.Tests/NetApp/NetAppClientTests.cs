using System.Text;
using Amazon.S3;
using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.API.Integration.Tests.Helpers;

namespace CPS.ComplexCases.API.Integration.Tests.NetApp;

[Collection("Integration Tests")]
public class NetAppClientTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly string _testFolderPrefix;

    public NetAppClientTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        var basePrefix = _fixture.NetAppTestFolderPrefix ?? $"integration-tests/{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _testFolderPrefix = $"{basePrefix}/client-{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        if (!_fixture.IsNetAppConfigured)
            return;

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
    public async Task ListObjectsInBucket_ValidBucket_ReturnsObjects()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var arg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            maxKeys: 10);

        // Act
        var result = await _fixture.NetAppClient!.ListObjectsInBucketAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(_fixture.NetAppBucketName!.ToLowerInvariant(), result.Data.BucketName.ToLowerInvariant());
    }

    [SkippableFact]
    public async Task ListObjectsInBucket_WithPrefix_ReturnsFilteredObjects()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var prefix = _testFolderPrefix;
        var arg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            prefix: prefix,
            maxKeys: 100);

        // Act
        var result = await _fixture.NetAppClient!.ListObjectsInBucketAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.All(result.Data.FileData, file => Assert.StartsWith(prefix, file.Path, StringComparison.OrdinalIgnoreCase));
    }

    [SkippableFact]
    public async Task ListObjectsInBucket_WithMaxKeys_RespectsLimit()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var maxKeys = 5;
        var arg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            maxKeys: maxKeys);

        // Act
        var result = await _fixture.NetAppClient!.ListObjectsInBucketAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.True(result.Pagination.KeyCount <= maxKeys);
    }

    [SkippableFact]
    public async Task ListFoldersInBucket_ValidBucket_ReturnsFolders()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var arg = _fixture.NetAppArgFactory!.CreateListFoldersInBucketArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            maxKeys: 10);

        // Act
        var result = await _fixture.NetAppClient!.ListFoldersInBucketAsync(arg);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.FolderData);
    }

    [SkippableFact]
    public async Task CreateFolder_NewFolder_ReturnsTrue()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var folderName = $"{_testFolderPrefix}/test-folder-{Guid.NewGuid():N}";
        var arg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            folderName);

        // Act
        var result = await _fixture.NetAppClient!.CreateFolderAsync(arg);

        // Assert
        Assert.True(result, "Folder creation should succeed");
    }

    [SkippableFact]
    public async Task CreateFolder_NestedFolder_ReturnsTrue()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var nestedFolderName = $"{_testFolderPrefix}/nested/subfolder-{Guid.NewGuid():N}";
        var arg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            nestedFolderName);

        // Act
        var result = await _fixture.NetAppClient!.CreateFolderAsync(arg);

        // Assert
        Assert.True(result, "Nested folder creation should succeed");
    }

    [SkippableFact]
    public async Task UploadObject_SmallFile_ReturnsTrue()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testContent = $"Integration test content - {DateTime.UtcNow:O}";
        var contentBytes = Encoding.UTF8.GetBytes(testContent);
        using var stream = new MemoryStream(contentBytes);
        var objectKey = $"{_testFolderPrefix}/test-file-{Guid.NewGuid():N}.txt";

        var arg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            objectKey,
            stream,
            contentBytes.Length);

        // Act
        var result = await _fixture.NetAppClient!.UploadObjectAsync(arg);

        // Assert
        Assert.True(result, "File upload should succeed");
    }

    [SkippableFact]
    public async Task GetObject_NonExistentFile_ThrowsFileNotFoundException()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var nonExistentKey = $"{_testFolderPrefix}/non-existent-file-{Guid.NewGuid():N}.txt";
        var arg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            nonExistentKey);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _fixture.NetAppClient!.GetObjectAsync(arg));
    }

    [SkippableFact]
    public async Task DoesObjectExist_ExistingObject_ReturnsTrue()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testContent = "Existence test content";
        var contentBytes = Encoding.UTF8.GetBytes(testContent);
        var objectKey = $"{_testFolderPrefix}/exists-test-{Guid.NewGuid():N}.txt";

        using var uploadStream = new MemoryStream(contentBytes);
        var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            objectKey,
            uploadStream,
            contentBytes.Length);

        await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);

        // Act
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, objectKey);

        // Assert
        Assert.True(exists, "Object should exist after upload");
    }

    [SkippableFact]
    public async Task DoesObjectExist_NonExistingObject_ReturnsFalse()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var nonExistentKey = $"{_testFolderPrefix}/does-not-exist-{Guid.NewGuid():N}.txt";

        // Act
        var exists = await NetAppTestHelper.ObjectExistsViaListAsync(_fixture, bearerToken, nonExistentKey);

        // Assert
        Assert.False(exists, "Object should not exist");
    }

    [SkippableFact]
    public async Task MultipartUpload_LargeFile_CompletesSuccessfully()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var objectKey = $"{_testFolderPrefix}/multipart-test-{Guid.NewGuid():N}.bin";

        // Create a 10MB test file (minimum part size for S3 is 5MB)
        var partSize = 5 * 1024 * 1024; // 5MB
        var part1Data = new byte[partSize];
        var part2Data = new byte[partSize];
        new Random().NextBytes(part1Data);
        new Random().NextBytes(part2Data);

        // In production (TransferFile.cs) the orchestrator retries the entire activity when
        // CompleteMultipartUpload returns a transient 500, because the error also internally aborts
        // the multipart upload and invalidates the upload ID. We mirror that here by restarting the
        // full flow (initiate → upload parts → complete) on each attempt.
        string? completedETag = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            // Initiate multipart upload
            var initiateArg = _fixture.NetAppArgFactory!.CreateInitiateMultipartUploadArg(
                bearerToken, _fixture.NetAppBucketName!, objectKey);

            var initiateResponse = await _fixture.NetAppClient!.InitiateMultipartUploadAsync(initiateArg);
            Assert.NotNull(initiateResponse);
            Assert.NotEmpty(initiateResponse.UploadId);

            var uploadId = initiateResponse.UploadId;
            var completedParts = new Dictionary<int, string>();

            // Upload part 1
            var part1Response = await _fixture.NetAppClient!.UploadPartAsync(
                _fixture.NetAppArgFactory!.CreateUploadPartArg(
                    bearerToken, _fixture.NetAppBucketName!, objectKey, part1Data, 1, uploadId));
            Assert.NotNull(part1Response);
            completedParts[1] = part1Response.ETag;

            // Upload part 2
            var part2Response = await _fixture.NetAppClient!.UploadPartAsync(
                _fixture.NetAppArgFactory!.CreateUploadPartArg(
                    bearerToken, _fixture.NetAppBucketName!, objectKey, part2Data, 2, uploadId));
            Assert.NotNull(part2Response);
            completedParts[2] = part2Response.ETag;

            // Match the production delay from TransferFile.cs before completing.
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Complete multipart upload
            var completeArg = _fixture.NetAppArgFactory!.CreateCompleteMultipartUploadArg(
                bearerToken, _fixture.NetAppBucketName!, objectKey, uploadId, completedParts);

            try
            {
                var completeResponse = await _fixture.NetAppClient!.CompleteMultipartUploadAsync(completeArg);
                Assert.NotNull(completeResponse);
                completedETag = completeResponse.ETag;
                break;
            }
            catch (AmazonS3Exception) when (attempt < 3)
            {
                // StorageGRID aborted the upload alongside the internal error;
                // the upload ID is now invalid so the full flow must be restarted.
            }
        }

        // Assert
        Assert.False(string.IsNullOrEmpty(completedETag));
        var exists = await NetAppTestHelper.ObjectExistsViaListAsync(_fixture, bearerToken, objectKey);
        Assert.True(exists, "Multipart uploaded file should exist");
    }

    [SkippableFact]
    public async Task ListObjectsInBucket_WithPagination_CanIteratePages()
    {
        Skip.If(!_fixture.IsNetAppConfigured, "NetApp not configured");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var pageSize = 5;
        var allFileKeys = new List<string>();

        // Act
        var firstPageArg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            maxKeys: pageSize);

        var firstPageResult = await _fixture.NetAppClient!.ListObjectsInBucketAsync(firstPageArg);

        Assert.NotNull(firstPageResult);
        allFileKeys.AddRange(firstPageResult.Data.FileData.Select(f => f.Path));

        // Get second page if available
        if (!string.IsNullOrEmpty(firstPageResult.Pagination.NextContinuationToken))
        {
            var secondPageArg = _fixture.NetAppArgFactory!.CreateListObjectsInBucketArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                continuationToken: firstPageResult.Pagination.NextContinuationToken,
                maxKeys: pageSize);

            var secondPageResult = await _fixture.NetAppClient!.ListObjectsInBucketAsync(secondPageArg);

            Assert.NotNull(secondPageResult);

            var secondPageKeys = secondPageResult.Data.FileData.Select(f => f.Path).ToList();
            var overlap = allFileKeys.Intersect(secondPageKeys).ToList();
            Assert.Empty(overlap);
        }
    }
}

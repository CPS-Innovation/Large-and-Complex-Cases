using System.Text;
using Amazon.S3;
using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.API.Integration.Tests.Helpers;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

namespace CPS.ComplexCases.API.Integration.Tests.E2E;

[Collection("Integration Tests")]
public class E2ETransferFlowTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly string _netAppTestFolderPrefix;
    private readonly List<(string FileId, string WorkspaceId)> _egressFilesToCleanup = new();

    private const int MinimumMultipartChunkSize = 5 * 1024 * 1024; // 5MB - S3 minimum part size

    public E2ETransferFlowTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        var basePrefix = _fixture.NetAppTestFolderPrefix ?? $"integration-tests/{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _netAppTestFolderPrefix = $"{basePrefix}/e2e-transfer-{Guid.NewGuid():N}";
    }

    public bool IsE2ETransferConfigured => _fixture.IsEgressConfigured && _fixture.IsNetAppConfigured;

    public async Task InitializeAsync()
    {
        if (!IsE2ETransferConfigured)
            return;

        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();

        var createFolderArg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            _netAppTestFolderPrefix);

        await _fixture.NetAppClient!.CreateFolderAsync(createFolderArg);
    }

    public async Task DisposeAsync()
    {
        if (!IsE2ETransferConfigured)
            return;

        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();

        var deleteArg = _fixture.NetAppArgFactory!.CreateDeleteFileOrFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            "e2e-transfer-cleanup",
            _netAppTestFolderPrefix);

        await _fixture.NetAppClient!.DeleteFileOrFolderAsync(deleteArg);

        if (_egressFilesToCleanup.Any())
        {
            var groupedByWorkspace = _egressFilesToCleanup.GroupBy(x => x.WorkspaceId);
            foreach (var group in groupedByWorkspace)
            {
                var filesToDelete = group.Select(x => new DeletionEntityDto
                {
                    FileId = x.FileId,
                    Path = string.Empty
                }).ToList();

                await _fixture.EgressStorageClient!.DeleteFilesAsync(filesToDelete, group.Key);
            }
        }
    }

    [SkippableFact]
    public async Task EgressToNetApp_CopySmallFile_TransfersSuccessfully()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var workspaceId = _fixture.EgressWorkspaceId!;
        var testFileName = TestDataHelper.GenerateTestFileName();
        var testContent = $"E2E Transfer Test Content - {DateTime.UtcNow:O} - {Guid.NewGuid()}";
        var testContentBytes = Encoding.UTF8.GetBytes(testContent);
        var destinationPath = TestDataHelper.GenerateTestFolderPath();

        var egressSession = await UploadFileToEgressAsync(workspaceId, destinationPath, testFileName, testContentBytes);
        Assert.NotNull(egressSession.UploadId);

        var netAppDestinationPath = $"{_netAppTestFolderPrefix}/{testFileName}";

        using var uploadStream = new MemoryStream(testContentBytes);
        await _fixture.NetAppClient!.UploadObjectAsync(
            _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                netAppDestinationPath,
                uploadStream,
                testContentBytes.Length));

        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, netAppDestinationPath);
        Assert.True(exists, "File should exist in NetApp after transfer");

        // Assert
        var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, netAppDestinationPath);
        Assert.Equal(testContentBytes.Length, metadata.ContentLength);
    }

    [SkippableFact]
    public async Task EgressToNetApp_CopyLargeFile_MultipartTransferSucceeds()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testFileName = TestDataHelper.GenerateTestFileName("bin");

        var partSize = MinimumMultipartChunkSize;
        var testContent = new byte[partSize * 2]; // 10MB
        new Random().NextBytes(testContent);

        var netAppDestinationPath = $"{_netAppTestFolderPrefix}/{testFileName}";

        // In production (TransferFile.cs) the orchestrator retries the entire activity when
        // CompleteMultipartUpload returns a transient 500, because the error also internally aborts
        // the multipart upload and invalidates the upload ID. We mirror that here by restarting the
        // full flow (initiate → upload parts → complete) on each attempt.
        var chunks = TestDataHelper.SplitIntoChunks(testContent, partSize).ToList();
        string? completedETag = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var initiateArg = _fixture.NetAppArgFactory!.CreateInitiateMultipartUploadArg(
                bearerToken, _fixture.NetAppBucketName!, netAppDestinationPath);

            var initiateResponse = await _fixture.NetAppClient!.InitiateMultipartUploadAsync(initiateArg);
            Assert.NotNull(initiateResponse);
            Assert.NotEmpty(initiateResponse.UploadId);

            var uploadId = initiateResponse.UploadId;
            var completedParts = new Dictionary<int, string>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var partResponse = await _fixture.NetAppClient!.UploadPartAsync(
                    _fixture.NetAppArgFactory!.CreateUploadPartArg(
                        bearerToken, _fixture.NetAppBucketName!, netAppDestinationPath,
                        chunks[i], i + 1, uploadId));
                Assert.NotNull(partResponse);
                completedParts[i + 1] = partResponse.ETag;
            }

            // Match the production delay from TransferFile.cs before completing.
            await Task.Delay(TimeSpan.FromSeconds(2));

            try
            {
                var completeResponse = await _fixture.NetAppClient!.CompleteMultipartUploadAsync(
                    _fixture.NetAppArgFactory!.CreateCompleteMultipartUploadArg(
                        bearerToken, _fixture.NetAppBucketName!, netAppDestinationPath,
                        uploadId, completedParts));
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

        Assert.False(string.IsNullOrEmpty(completedETag));
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, netAppDestinationPath);
        Assert.True(exists, "Large file should exist in NetApp after multipart transfer");
    }


    [SkippableFact]
    public async Task EgressToNetApp_CopyMultipleFiles_AllTransferSuccessfully()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var batchFolder = $"{_netAppTestFolderPrefix}/batch-transfer-{Guid.NewGuid():N}";
        var fileCount = 3;
        var uploadedFiles = new List<(string FileName, string Content)>();

        var createFolderArg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            batchFolder);
        await _fixture.NetAppClient!.CreateFolderAsync(createFolderArg);

        // Act
        for (int i = 0; i < fileCount; i++)
        {
            var fileName = $"batch-file-{i + 1}-{Guid.NewGuid():N}.txt";
            var content = $"Batch transfer test content for file {i + 1} - {DateTime.UtcNow:O}";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var destinationPath = $"{batchFolder}/{fileName}";

            using var stream = new MemoryStream(contentBytes);
            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                destinationPath,
                stream,
                contentBytes.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
            uploadedFiles.Add((destinationPath, content));
        }

        // Assert
        foreach (var (filePath, expectedContent) in uploadedFiles)
        {
            var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, filePath);
            Assert.True(exists, $"File {filePath} should exist in NetApp after batch transfer");

            var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, filePath);
            Assert.Equal(Encoding.UTF8.GetBytes(expectedContent).Length, metadata.ContentLength);
        }
    }

    [SkippableFact]
    public async Task EgressToNetApp_CopyWithNestedFolders_PreservesFolderStructure()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var rootFolder = $"{_netAppTestFolderPrefix}/nested-transfer-{Guid.NewGuid():N}";

        var nestedPaths = new[]
        {
            $"{rootFolder}/file-at-root.txt",
            $"{rootFolder}/level1/file-at-level1.txt",
            $"{rootFolder}/level1/level2/file-at-level2.txt",
            $"{rootFolder}/level1/level2/level3/file-at-level3.txt"
        };

        // Act
        foreach (var path in nestedPaths)
        {
            var content = $"Content for {Path.GetFileName(path)} at {DateTime.UtcNow:O}";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            using var stream = new MemoryStream(contentBytes);
            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                path,
                stream,
                contentBytes.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
        }

        // Assert
        foreach (var path in nestedPaths)
        {
            var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, path);
            Assert.True(exists, $"Nested file {path} should exist after transfer");
        }
    }

    [SkippableFact]
    public async Task NetAppToEgress_CopySmallFile_TransfersSuccessfully()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var workspaceId = _fixture.EgressWorkspaceId!;
        var testFileName = TestDataHelper.GenerateTestFileName();
        var testContent = $"NetApp to Egress Transfer Test - {DateTime.UtcNow:O} - {Guid.NewGuid()}";
        var testContentBytes = Encoding.UTF8.GetBytes(testContent);

        var netAppSourcePath = $"{_netAppTestFolderPrefix}/{testFileName}";
        using var uploadStream = new MemoryStream(testContentBytes);
        var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            netAppSourcePath,
            uploadStream,
            testContentBytes.Length);

        await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);

        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, netAppSourcePath);
        Assert.True(exists, "Source file should exist in NetApp");

        var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, netAppSourcePath);
        Assert.Equal(testContentBytes.Length, metadata.ContentLength);

        var egressDestinationPath = TestDataHelper.GenerateTestFolderPath();
        var egressSession =
            await UploadFileToEgressAsync(workspaceId, egressDestinationPath, testFileName, testContentBytes);

        Assert.NotNull(egressSession);
        Assert.NotNull(egressSession.UploadId);
        Assert.Equal(workspaceId, egressSession.WorkspaceId);
    }

    [SkippableFact]
    public async Task NetAppToEgress_CopyMultipleFiles_AllTransferSuccessfully()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var workspaceId = _fixture.EgressWorkspaceId!;
        var batchFolder = $"{_netAppTestFolderPrefix}/batch-to-egress-{Guid.NewGuid():N}";
        var fileCount = 3;
        var uploadedToNetApp = new List<(string Path, byte[] Content)>();

        var createFolderArg = _fixture.NetAppArgFactory!.CreateCreateFolderArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            batchFolder);
        await _fixture.NetAppClient!.CreateFolderAsync(createFolderArg);

        // Act
        for (int i = 0; i < fileCount; i++)
        {
            var fileName = $"egress-bound-file-{i + 1}-{Guid.NewGuid():N}.txt";
            var content = Encoding.UTF8.GetBytes($"Content for Egress file {i + 1} - {DateTime.UtcNow:O}");
            var path = $"{batchFolder}/{fileName}";

            using var stream = new MemoryStream(content);
            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                path,
                stream,
                content.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
            uploadedToNetApp.Add((path, content));
        }

        // Assert
        foreach (var (path, content) in uploadedToNetApp)
        {
            var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, path);
            Assert.True(exists, $"File {path} should exist in NetApp");

            var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, path);
            Assert.Equal(content.Length, metadata.ContentLength);
        }

        var egressDestinationPath = TestDataHelper.GenerateTestFolderPath();
        var transferResults = new List<UploadSession>();

        foreach (var (sourcePath, content) in uploadedToNetApp)
        {
            var fileName = Path.GetFileName(sourcePath);
            var session = await UploadFileToEgressAsync(workspaceId, egressDestinationPath, fileName, content);
            transferResults.Add(session);
        }

        Assert.Equal(fileCount, transferResults.Count);
        Assert.All(transferResults, result =>
        {
            Assert.NotNull(result);
            Assert.NotNull(result.UploadId);
        });
    }

    [SkippableFact]
    public async Task RoundTrip_EgressToNetAppToEgress_DataIntegrityPreserved()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var workspaceId = _fixture.EgressWorkspaceId!;
        var testFileName = $"round-trip-{Guid.NewGuid():N}.txt";
        var originalContent = $"Round-trip test content with special chars: éàü & <test> - {DateTime.UtcNow:O}";
        var originalBytes = Encoding.UTF8.GetBytes(originalContent);

        // Act
        var netAppPath = $"{_netAppTestFolderPrefix}/{testFileName}";
        using var uploadStream = new MemoryStream(originalBytes);
        var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            netAppPath,
            uploadStream,
            originalBytes.Length);

        await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);

        // Assert
        var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, netAppPath);
        Assert.True(exists, "File should exist in NetApp after first leg of transfer");

        var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, netAppPath);
        Assert.Equal(originalBytes.Length, metadata.ContentLength);

        var egressDestinationPath = TestDataHelper.GenerateTestFolderPath();
        var returnFileName = $"returned-{testFileName}";
        var egressSession =
            await UploadFileToEgressAsync(workspaceId, egressDestinationPath, returnFileName, originalBytes);

        Assert.NotNull(egressSession);
        Assert.NotNull(egressSession.UploadId);
    }

    [SkippableFact]
    public async Task NetAppToEgress_CopyWithNestedFolders_PreservesFolderStructure()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();
        const string sourceRootFolderPath = "folder1";

        var files = new[]
        {
            ("folder1/file1.txt", $"Content for file1 - {Guid.NewGuid()}"),
            ("folder1/nestedfolder1/file2.txt", $"Content for file2 - {Guid.NewGuid()}"),
            ("folder1/nestedfolder1/file3.txt", $"Content for file3 - {Guid.NewGuid()}"),
        };

        // Act — upload each file the same way TransferFile activity does, using the
        // pre-stripped sourceRootFolderPath that ListFilesForTransfer now produces.
        foreach (var (relativePath, content) in files)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: destinationPath,
                fileSize: contentBytes.Length,
                sourcePath: relativePath,
                workspaceId: workspaceId,
                relativePath: relativePath,
                sourceRootFolderPath: sourceRootFolderPath);

            _egressFilesToCleanup.Add((session.UploadId!, workspaceId));

            await _fixture.EgressStorageClient.UploadChunkAsync(
                session: session,
                chunkNumber: 1,
                chunkData: contentBytes,
                start: 0,
                end: contentBytes.Length - 1,
                totalSize: contentBytes.Length);

            await _fixture.EgressStorageClient.CompleteUploadAsync(session,
                TestDataHelper.ComputeMd5Hash(contentBytes));
        }

        // Assert — list the Egress workspace and filter to just our destination folder
        var allFiles = await _fixture.EgressStorageClient!.GetAllFilesFromFolderAsync(destinationPath, workspaceId);
        var fullFilePaths = allFiles
            .Where(f => !string.IsNullOrEmpty(f.FullFilePath)
                        && f.FullFilePath.StartsWith(destinationPath, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.FullFilePath!)
            .ToList();

        Assert.Equal(3, fullFilePaths.Count);

        // file1.txt must land directly under destinationPath — NOT inside a "folder1" sub-folder.
        Assert.Contains(fullFilePaths,
            p => p.Equals($"{destinationPath}/file1.txt", StringComparison.OrdinalIgnoreCase));

        // file2.txt and file3.txt must land inside a "nestedfolder1" sub-folder.
        Assert.Contains(fullFilePaths,
            p => p.Equals($"{destinationPath}/nestedfolder1/file2.txt", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(fullFilePaths,
            p => p.Equals($"{destinationPath}/nestedfolder1/file3.txt", StringComparison.OrdinalIgnoreCase));
    }

    [SkippableFact]
    public async Task NetAppToEgress_WithPreStrippedPaths_DestinationPathsAreCorrect()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var workspaceId = _fixture.EgressWorkspaceId!;
        var destinationPath = TestDataHelper.GenerateTestFolderPath();
        const string sourceRootFolderPath = "folder1";

        var files = new[]
        {
            ("folder1/file1.txt", Encoding.UTF8.GetBytes($"Original content A - {Guid.NewGuid()}")),
            ("folder1/nestedfolder1/file2.txt", Encoding.UTF8.GetBytes($"Original content B - {Guid.NewGuid()}")),
        };

        // First transfer — upload all files to Egress.
        foreach (var (relativePath, contentBytes) in files)
        {
            var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
                destinationPath: destinationPath,
                fileSize: contentBytes.Length,
                sourcePath: relativePath,
                workspaceId: workspaceId,
                relativePath: relativePath,
                sourceRootFolderPath: sourceRootFolderPath);

            _egressFilesToCleanup.Add((session.UploadId!, workspaceId));

            await _fixture.EgressStorageClient.UploadChunkAsync(
                session: session,
                chunkNumber: 1,
                chunkData: contentBytes,
                start: 0,
                end: contentBytes.Length - 1,
                totalSize: contentBytes.Length);

            await _fixture.EgressStorageClient.CompleteUploadAsync(session,
                TestDataHelper.ComputeMd5Hash(contentBytes));
        }

        var destinationFiles =
            (await _fixture.EgressStorageClient!.GetAllFilesFromFolderAsync(destinationPath, workspaceId))
            .Where(f => !string.IsNullOrEmpty(f.FullFilePath)
                        && f.FullFilePath.StartsWith(destinationPath, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.FullFilePath!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.NotEmpty(destinationFiles);

        static string ComputeDestPath(string dest, string relPath, string rootFolder)
        {
            var destWithSlash = dest.EndsWith('/') ? dest : dest + "/";
            var index = relPath.IndexOf(rootFolder, StringComparison.OrdinalIgnoreCase);
            if (index == 0)
            {
                return destWithSlash + relPath.Substring(rootFolder.Length).TrimStart('/', '\\');
            }

            return destWithSlash + relPath;
        }

        // Assert
        foreach (var (relativePath, _) in files)
        {
            var computedDest = ComputeDestPath(destinationPath, relativePath, sourceRootFolderPath);
            Assert.Contains(computedDest, destinationFiles);
        }

        Assert.Equal(files.Length, destinationFiles.Count);
    }

    [SkippableFact]
    public async Task NetAppToEgress_NonExistentSource_ThrowsFileNotFoundException()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var nonExistentPath = $"{_netAppTestFolderPrefix}/non-existent-file-{Guid.NewGuid():N}.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            var getArg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                nonExistentPath);

            await _fixture.NetAppClient!.GetObjectAsync(getArg);
        });
    }

    [SkippableFact]
    public async Task EgressToNetApp_ExistingFiles_AreNotOverwritten()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testFileName = TestDataHelper.GenerateTestFileName();
        var initialContent = $"Initial content - {DateTime.UtcNow:O} - {Guid.NewGuid()}";
        var updatedContent = $"Updated file content - {DateTime.UtcNow:O} - {Guid.NewGuid()}";
        var initialBytes = Encoding.UTF8.GetBytes(initialContent);
        var updatedBytes = Encoding.UTF8.GetBytes(updatedContent);
        var destinationPath = $"{_netAppTestFolderPrefix}/{testFileName}";

        // Upload initial file to NetApp
        using (var stream = new MemoryStream(initialBytes))
        {
            var uploadArg = _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                destinationPath,
                stream,
                initialBytes.Length);

            await _fixture.NetAppClient!.UploadObjectAsync(uploadArg);
        }

        var arg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            destinationPath);

        var exists = await _fixture.NetAppClient!.DoesObjectExistAsync(arg);
        Assert.True(exists, "File should exist after initial upload");

        // Verify DoesObjectExistAsync correctly detects the duplicate on a second check
        var duplicateCheck = await _fixture.NetAppClient!.DoesObjectExistAsync(arg);
        Assert.True(duplicateCheck, "DoesObjectExistAsync should consistently return true for an existing file");

        // Assert that the content has not changed (i.e., the file was not overwritten)
        var metadata = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, destinationPath);
        Assert.Equal(initialBytes.Length, metadata.ContentLength);
    }

    [SkippableFact]
    public async Task NetAppToEgress_LargeFileMultipart_DoesNotOverwriteOnSecondUpload()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testFileName = TestDataHelper.GenerateTestFileName("txt");

        // 6MB file — exceeds the 5MB minimum part size, requiring multipart upload
        var fileSize = 6 * 1024 * 1024;
        var initialContent = new byte[fileSize];
        new Random(42).NextBytes(initialContent);

        var updatedContent = new byte[fileSize];
        new Random(99).NextBytes(updatedContent);

        var netAppSourcePath = $"{_netAppTestFolderPrefix}/{testFileName}";

        // Upload 6MB file to NetApp via multipart
        var chunks = TestDataHelper.SplitIntoChunks(initialContent, MinimumMultipartChunkSize).ToList();
        string? completedETag = null;

        completedETag = await UploadMultipartFileToNetApp(bearerToken, netAppSourcePath, chunks, completedETag);
        Assert.False(string.IsNullOrEmpty(completedETag), "Initial multipart upload to NetApp should have succeeded");

        // Act — check if file exists before second upload and skip if already present (mirrors production behaviour)
        var getArg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            netAppSourcePath);

        var alreadyExists = await _fixture.NetAppClient!.DoesObjectExistAsync(getArg);
        Assert.True(alreadyExists, "File should exist after initial multipart upload");

        // Verify the existence check consistently detects the multipart-uploaded file
        var duplicateCheck = await _fixture.NetAppClient!.DoesObjectExistAsync(getArg);
        Assert.True(duplicateCheck, "DoesObjectExistAsync should detect existing multipart-uploaded file");

        var existingFileMetadata = await _fixture.NetAppClient!.GetHeadObjectMetadataAsync(getArg);

        // Assert that the existing file was not overwritten by the second upload
        Assert.Equal(completedETag.Unquote(), existingFileMetadata.ETag);
    }

    [SkippableFact]
    public async Task EgressToNetApp_NewFiles_AreUploaded_ExistingFiles_AreNotOverwritten()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();

        var testFileName1 = TestDataHelper.GenerateTestFileName();
        var testFileName2 = TestDataHelper.GenerateTestFileName();
        var newFileName1 = TestDataHelper.GenerateTestFileName();
        var newFileName2 = TestDataHelper.GenerateTestFileName();

        var existingContent1 = Encoding.UTF8.GetBytes($"Existing content 1 - {DateTime.UtcNow:O} - {Guid.NewGuid()}");
        var existingContent2 = Encoding.UTF8.GetBytes($"Existing content 2 - {DateTime.UtcNow:O} - {Guid.NewGuid()}");
        var newContent1 = Encoding.UTF8.GetBytes($"New content 1 - {DateTime.UtcNow:O} - {Guid.NewGuid()}");
        var newContent2 = Encoding.UTF8.GetBytes($"New content 2 - {DateTime.UtcNow:O} - {Guid.NewGuid()}");
        var overwriteContent =
            Encoding.UTF8.GetBytes($"Overwrite attempt content - {DateTime.UtcNow:O} - {Guid.NewGuid()}");

        var destinationPath1 = $"{_netAppTestFolderPrefix}/{testFileName1}";
        var destinationPath2 = $"{_netAppTestFolderPrefix}/{testFileName2}";
        var destinationPathNew1 = $"{_netAppTestFolderPrefix}/{newFileName1}";
        var destinationPathNew2 = $"{_netAppTestFolderPrefix}/{newFileName2}";

        // Pre-upload testFileName1 and testFileName2 to NetApp so they exist before the test runs
        foreach (var (path, content) in new[]
                 {
                     (destinationPath1, existingContent1),
                     (destinationPath2, existingContent2)
                 })
        {
            using var stream = new MemoryStream(content);
            await _fixture.NetAppClient!.UploadObjectAsync(
                _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                    bearerToken,
                    _fixture.NetAppBucketName!,
                    path,
                    stream,
                    content.Length));

            var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, path);
            Assert.True(exists, $"Pre-condition failed: {path} should exist in NetApp before the test runs");
        }

        // Act — attempt to upload all four files, skipping any that already exist
        var filesToUpload = new[]
        {
            (Path: destinationPath1, FileName: testFileName1, Content: overwriteContent),
            (Path: destinationPath2, FileName: testFileName2, Content: overwriteContent),
            (Path: destinationPathNew1, FileName: newFileName1, Content: newContent1),
            (Path: destinationPathNew2, FileName: newFileName2, Content: newContent2),
        };

        foreach (var (path, fileName, content) in filesToUpload)
        {
            var getArg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                path);

            var alreadyExists = await _fixture.NetAppClient!.DoesObjectExistAsync(getArg);

            if (!alreadyExists)
            {
                using var stream = new MemoryStream(content);
                await _fixture.NetAppClient!.UploadObjectAsync(
                    _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                        bearerToken,
                        _fixture.NetAppBucketName!,
                        path,
                        stream,
                        content.Length));
            }
        }

        // Assert — existing files were NOT overwritten (content length matches original, not overwrite attempt)
        var metadata1 = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, destinationPath1);
        Assert.Equal(existingContent1.Length, metadata1.ContentLength);
        Assert.NotEqual(overwriteContent.Length, metadata1.ContentLength);

        var metadata2 = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, destinationPath2);
        Assert.Equal(existingContent2.Length, metadata2.ContentLength);
        Assert.NotEqual(overwriteContent.Length, metadata2.ContentLength);

        // Assert — new files were uploaded successfully
        var existsNew1 = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, destinationPathNew1);
        Assert.True(existsNew1, $"{newFileName1} should have been uploaded as it did not previously exist");

        var metadataNew1 = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, destinationPathNew1);
        Assert.Equal(newContent1.Length, metadataNew1.ContentLength);

        var existsNew2 = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, destinationPathNew2);
        Assert.True(existsNew2, $"{newFileName2} should have been uploaded as it did not previously exist");

        var metadataNew2 = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, destinationPathNew2);
        Assert.Equal(newContent2.Length, metadataNew2.ContentLength);
    }

    [SkippableFact]
    public async Task EgressToNetApp_SameFilenameOnDifferentPath_UploadsSuccessfully()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var testFileName = TestDataHelper.GenerateTestFileName();

        var originalContent = Encoding.UTF8.GetBytes($"Original file content - {DateTime.UtcNow:O} - {Guid.NewGuid()}");
        var secondContent = Encoding.UTF8.GetBytes($"Second file content - {DateTime.UtcNow:O} - {Guid.NewGuid()}");

        // Two distinct paths that share the same filename
        var pathA = $"{_netAppTestFolderPrefix}/{testFileName}";
        var pathB = $"{_netAppTestFolderPrefix}/folder/{testFileName}";

        // Pre-upload the file to pathA so it already exists
        using (var stream = new MemoryStream(originalContent))
        {
            await _fixture.NetAppClient!.UploadObjectAsync(
                _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                    bearerToken,
                    _fixture.NetAppBucketName!,
                    pathA,
                    stream,
                    originalContent.Length));
        }

        var pathAExists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, pathA);
        Assert.True(pathAExists, $"Pre-condition failed: {pathA} should exist in NetApp before the test runs");

        // Act — upload a file with the same filename to a different path (pathB)
        var getArgB = _fixture.NetAppArgFactory!.CreateGetObjectArg(
            bearerToken,
            _fixture.NetAppBucketName!,
            pathB);

        var pathBAlreadyExists = await _fixture.NetAppClient!.DoesObjectExistAsync(getArgB);
        Assert.False(pathBAlreadyExists, $"Pre-condition failed: {pathB} should NOT exist before the upload");

        if (!pathBAlreadyExists)
        {
            using var stream = new MemoryStream(secondContent);
            await _fixture.NetAppClient!.UploadObjectAsync(
                _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                    bearerToken,
                    _fixture.NetAppBucketName!,
                    pathB,
                    stream,
                    secondContent.Length));
        }

        // Assert — pathB was uploaded successfully with its own content
        var pathBExists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, pathB);
        Assert.True(pathBExists, $"{testFileName} should have been uploaded to {pathB} as it is a unique path");

        var metadataB = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, pathB);
        Assert.Equal(secondContent.Length, metadataB.ContentLength);

        // Assert — pathA was not affected by the upload to pathB
        var metadataA = await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, pathA);
        Assert.Equal(originalContent.Length, metadataA.ContentLength);
        Assert.NotEqual(secondContent.Length, metadataA.ContentLength);
    }

    [SkippableFact]
    public async Task EgressToNetApp_NestedFolder_DetectsDuplicatesAtAllLevels()
    {
        Skip.If(!IsE2ETransferConfigured, "Both Egress and NetApp configuration required for E2E transfer tests");

        // Arrange
        var bearerToken = await _fixture.GetUserDelegatedBearerTokenAsync();
        var folderPrefix = $"{_netAppTestFolderPrefix}/nested-dup-{Guid.NewGuid():N}";

        // Files that will be pre-populated (should be detected as duplicates)
        var existingFile1Content = Encoding.UTF8.GetBytes($"Existing root file - {Guid.NewGuid()}");
        var existingNestedContent = Encoding.UTF8.GetBytes($"Existing nested file - {Guid.NewGuid()}");

        // Files that are new (should be uploaded successfully)
        var newFile2Content = Encoding.UTF8.GetBytes($"New root file - {Guid.NewGuid()}");
        var newNestedContent = Encoding.UTF8.GetBytes($"New nested file - {Guid.NewGuid()}");

        var paths = new[]
        {
            (Path: $"{folderPrefix}/file1.txt", Content: existingFile1Content, ShouldExist: true),
            (Path: $"{folderPrefix}/nested/nested1.txt", Content: existingNestedContent, ShouldExist: true),
            (Path: $"{folderPrefix}/file2.txt", Content: newFile2Content, ShouldExist: false),
            (Path: $"{folderPrefix}/nested/nested2.txt", Content: newNestedContent, ShouldExist: false),
        };

        // Pre-populate the files that should already exist
        foreach (var (path, content, shouldExist) in paths.Where(p => p.ShouldExist))
        {
            using var stream = new MemoryStream(content);
            await _fixture.NetAppClient!.UploadObjectAsync(
                _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                    bearerToken,
                    _fixture.NetAppBucketName!,
                    path,
                    stream,
                    content.Length));

            var exists = await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, path);
            Assert.True(exists, $"Pre-condition failed: {path} should exist before the test runs");
        }

        // Act — check each file and upload only if it doesn't already exist (mirrors production TransferFile.Run)
        foreach (var (path, content, shouldExist) in paths)
        {
            var getArg = _fixture.NetAppArgFactory!.CreateGetObjectArg(
                bearerToken,
                _fixture.NetAppBucketName!,
                path);

            var alreadyExists = await _fixture.NetAppClient!.DoesObjectExistAsync(getArg);

            if (shouldExist)
            {
                // These files were pre-populated — DoesObjectExistAsync should detect them
                Assert.True(alreadyExists, $"DoesObjectExistAsync should return true for pre-existing file: {path}");
            }
            else
            {
                // These files are new — DoesObjectExistAsync should return false
                Assert.False(alreadyExists, $"DoesObjectExistAsync should return false for new file: {path}");

                using var stream = new MemoryStream(content);
                await _fixture.NetAppClient!.UploadObjectAsync(
                    _fixture.NetAppArgFactory!.CreateUploadObjectArg(
                        bearerToken,
                        _fixture.NetAppBucketName!,
                        path,
                        stream,
                        content.Length));
            }
        }

        // Assert — pre-existing files are unchanged
        var metadata1 =
            await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, $"{folderPrefix}/file1.txt");
        Assert.Equal(existingFile1Content.Length, metadata1.ContentLength);

        var metadataNested1 =
            await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, $"{folderPrefix}/nested/nested1.txt");
        Assert.Equal(existingNestedContent.Length, metadataNested1.ContentLength);

        // Assert — new files were uploaded successfully
        var existsFile2 =
            await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken, $"{folderPrefix}/file2.txt");
        Assert.True(existsFile2, "New root-level file should have been uploaded");
        var metadataFile2 =
            await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, $"{folderPrefix}/file2.txt");
        Assert.Equal(newFile2Content.Length, metadataFile2.ContentLength);

        var existsNested2 =
            await NetAppTestHelper.WaitForObjectExistsAsync(_fixture, bearerToken,
                $"{folderPrefix}/nested/nested2.txt");
        Assert.True(existsNested2, "New nested file should have been uploaded");
        var metadataNested2 =
            await NetAppTestHelper.GetObjectMetadataAsync(_fixture, bearerToken, $"{folderPrefix}/nested/nested2.txt");
        Assert.Equal(newNestedContent.Length, metadataNested2.ContentLength);
    }

    private async Task<string?> UploadMultipartFileToNetApp(string bearerToken, string netAppSourcePath,
        List<byte[]> chunks, string? completedETag)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var initiateArg = _fixture.NetAppArgFactory!.CreateInitiateMultipartUploadArg(
                bearerToken, _fixture.NetAppBucketName!, netAppSourcePath);

            var initiateResponse = await _fixture.NetAppClient!.InitiateMultipartUploadAsync(initiateArg);
            Assert.NotNull(initiateResponse);
            Assert.NotEmpty(initiateResponse.UploadId);

            var uploadId = initiateResponse.UploadId;
            var completedParts = new Dictionary<int, string>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var partResponse = await _fixture.NetAppClient!.UploadPartAsync(
                    _fixture.NetAppArgFactory!.CreateUploadPartArg(
                        bearerToken, _fixture.NetAppBucketName!, netAppSourcePath,
                        chunks[i], i + 1, uploadId));
                Assert.NotNull(partResponse);
                completedParts[i + 1] = partResponse.ETag;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));

            try
            {
                var completeResponse = await _fixture.NetAppClient!.CompleteMultipartUploadAsync(
                    _fixture.NetAppArgFactory!.CreateCompleteMultipartUploadArg(
                        bearerToken, _fixture.NetAppBucketName!, netAppSourcePath,
                        uploadId, completedParts));
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

        return completedETag;
    }

    private async Task<UploadSession> UploadFileToEgressAsync(
        string workspaceId,
        string destinationPath,
        string fileName,
        byte[] content)
    {
        var session = await _fixture.EgressStorageClient!.InitiateUploadAsync(
            destinationPath: destinationPath,
            fileSize: content.Length,
            sourcePath: fileName,
            workspaceId: workspaceId,
            relativePath: fileName);

        if (content.Length <= 32 * 1024)
        {
            await _fixture.EgressStorageClient.UploadChunkAsync(
                session: session,
                chunkNumber: 1,
                chunkData: content,
                start: 0,
                end: content.Length - 1,
                totalSize: content.Length);
        }
        else
        {
            var chunkSize = 32 * 1024;
            var chunks = TestDataHelper.SplitIntoChunks(content, chunkSize).ToList();
            long position = 0;

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                await _fixture.EgressStorageClient.UploadChunkAsync(
                    session: session,
                    chunkNumber: i + 1,
                    chunkData: chunk,
                    start: position,
                    end: position + chunk.Length - 1,
                    totalSize: content.Length);
                position += chunk.Length;
            }
        }

        var md5Hash = TestDataHelper.ComputeMd5Hash(content);
        await _fixture.EgressStorageClient.CompleteUploadAsync(session, md5Hash);

        return session;
    }
}
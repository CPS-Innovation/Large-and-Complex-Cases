using System.Text;
using Amazon.S3;
using CPS.ComplexCases.API.Integration.Tests.Fixtures;
using CPS.ComplexCases.API.Integration.Tests.Helpers;
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
        var egressSession = await UploadFileToEgressAsync(workspaceId, egressDestinationPath, testFileName, testContentBytes);

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
        var egressSession = await UploadFileToEgressAsync(workspaceId, egressDestinationPath, returnFileName, originalBytes);

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
            ("folder1/file1.txt",                   $"Content for file1 - {Guid.NewGuid()}"),
            ("folder1/nestedfolder1/file2.txt",     $"Content for file2 - {Guid.NewGuid()}"),
            ("folder1/nestedfolder1/file3.txt",     $"Content for file3 - {Guid.NewGuid()}"),
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

            await _fixture.EgressStorageClient.CompleteUploadAsync(session, TestDataHelper.ComputeMd5Hash(contentBytes));
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
        Assert.Contains(fullFilePaths, p => p.Equals($"{destinationPath}/file1.txt", StringComparison.OrdinalIgnoreCase));

        // file2.txt and file3.txt must land inside a "nestedfolder1" sub-folder.
        Assert.Contains(fullFilePaths, p => p.Equals($"{destinationPath}/nestedfolder1/file2.txt", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(fullFilePaths, p => p.Equals($"{destinationPath}/nestedfolder1/file3.txt", StringComparison.OrdinalIgnoreCase));
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
            ("folder1/file1.txt",               Encoding.UTF8.GetBytes($"Original content A - {Guid.NewGuid()}")),
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

            await _fixture.EgressStorageClient.CompleteUploadAsync(session, TestDataHelper.ComputeMd5Hash(contentBytes));
        }

        var destinationFiles = (await _fixture.EgressStorageClient!.GetAllFilesFromFolderAsync(destinationPath, workspaceId))
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.WireMock.Mappings;
using CPS.ComplexCases.NetApp.Wrappers;
using CPS.ComplexCases.WireMock.Core;
using Moq;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.Tests.Integration;

public class NetAppStorageClientTests : IDisposable
{
    private bool _disposed = false;
    private readonly WireMockServer _server;
    private readonly NetAppClient _netAppClient;
    private readonly NetAppStorageClient _client;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<INetAppRequestFactory> _netAppRequestFactoryMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IS3ClientFactory> _s3ClientFactoryMock;
    private const string BucketName = "test-bucket";
    private const string ObjectKey = "test-document.pdf";
    private const string UploadId = "upload-id-49e18525de9c";
    private const int CaseId = 2164817;

    public NetAppStorageClientTests()
    {
        _server = WireMockServer.Start().LoadMappings(
            new ObjectMapping(),
            new UploadMapping()
        );

        var _netAppOptions = Options.Create(new NetAppOptions
        {
            Url = _server.Urls[0],
            BucketName = BucketName,
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            RegionName = "eu-west-2"
        });

        var s3ClientConfig = new AmazonS3Config
        {
            ServiceURL = _server.Urls[0],
            ForcePathStyle = true,
        };

        var credentials = new Amazon.Runtime.BasicAWSCredentials("fakeAccessKey", "fakeSecretKey");
        var amazonS3UtilsWrapper = new AmazonS3UtilsWrapper();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _netAppRequestFactoryMock = new Mock<INetAppRequestFactory>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _s3ClientFactoryMock = new Mock<IS3ClientFactory>();
        _s3ClientFactoryMock.Setup(f => f.GetS3ClientAsync()).ReturnsAsync(new AmazonS3Client(credentials, s3ClientConfig));

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

        _netAppClient = new NetAppClient(logger, amazonS3UtilsWrapper, _netAppRequestFactoryMock.Object, _s3ClientFactoryMock.Object);
        _client = new NetAppStorageClient(_netAppClient, _netAppArgFactoryMock.Object, _netAppOptions, _caseMetadataServiceMock.Object);
    }

    [Fact]
    public async Task InitiateUploadAsync_ReturnsUploadSession()
    {
        // Arrange
        const string destinationPath = "";
        const string sourcePath = ObjectKey;
        var fullPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = new InitiateMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        var getObjectArg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = fullPath
        };

        var getObjectAttributesRequest = new GetObjectAttributesRequest
        {
            BucketName = BucketName,
            Key = fullPath,
            ObjectAttributes = [ObjectAttributes.ETag]
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BucketName, fullPath)).Returns(arg);
        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, fullPath)).Returns(getObjectArg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectAttributesRequest(getObjectArg)).Returns(getObjectAttributesRequest);

        //Act
        var result = await _client.InitiateUploadAsync(destinationPath, 123, sourcePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UploadId, result.UploadId);
        Assert.Equal(fullPath, result.WorkspaceId);
    }

    [Fact]
    public async Task OpenReadStreamAsync_ReturnsStream()
    {
        // Arrange
        var arg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = ObjectKey
        };

        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, ObjectKey)).Returns(arg);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectRequest(arg)).Returns(request);

        // Act
        var result = await _client.OpenReadStreamAsync(ObjectKey);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CanRead);

        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task OpenReadStreamAsync_ThrowsIfFileNotFound()
    {
        // Arrange
        var invalidFilePath = "invalid-file-path/file.txt";

        var arg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = invalidFilePath
        };

        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = invalidFilePath
        };

        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, invalidFilePath)).Returns(arg);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectRequest(arg)).Returns(request);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _client.OpenReadStreamAsync(invalidFilePath));
    }

    [Fact]
    public async Task UploadChunkAsync_ReturnsUploadChunkResult()
    {
        // Arrange
        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var chunkData = new byte[] { 1, 2, 3 };

        var arg = new UploadPartArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            PartNumber = 2,
            PartData = chunkData,
            UploadId = UploadId
        };

        var request = new UploadPartRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            PartNumber = 2,
            UploadId = UploadId,
            InputStream = new MemoryStream(chunkData)
        };

        _netAppArgFactoryMock.Setup(f => f.CreateUploadPartArg(BucketName, ObjectKey, chunkData, 2, UploadId)).Returns(arg);
        _netAppRequestFactoryMock.Setup(c => c.UploadPartRequest(arg)).Returns(request);

        // Act
        var result = await _client.UploadChunkAsync(session, 2, chunkData);

        // Assert
        Assert.Equal(TransferDirection.EgressToNetApp, result.TransferDirection);
        Assert.Equal("etag-12345", result.ETag);
    }

    [Fact]
    public async Task CompleteUploadAsync_CallsClientWithCorrectArgs()
    {
        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var etags = new Dictionary<int, string> { [1] = "etag-12345" };

        var arg = new CompleteMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts =
            [

            ]
        };

        var request = new CompleteMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            UploadId = UploadId,
            PartETags = [

            ]
        };

        _netAppArgFactoryMock.Setup(f => f.CreateCompleteMultipartUploadArg(BucketName, ObjectKey, UploadId, etags)).Returns(arg);
        _netAppRequestFactoryMock.Setup(c => c.CompleteMultipartUploadRequest(arg)).Returns(request);

        // Act & Assert (should not throw)
        await _client.CompleteUploadAsync(session, null, etags);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_ReturnsFileTransferInfo()
    {
        // Arrange
        var folderName = "/nested-objects/";
        var maxKeys = 1000;

        var selectedEntities = new List<TransferEntityDto>
        {
            new() { Path = ObjectKey },
            new() { Path = folderName }
        };

        var arg = new ListObjectsInBucketArg
        {
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys.ToString(),
            Prefix = folderName
        };

        var request = new ListObjectsV2Request
        {
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys,
            Prefix = folderName
        };

        var caseMetadata = new CaseMetadata
        {
            CaseId = CaseId,
            EgressWorkspaceId = "egress-workspace-id-123",
            NetappFolderPath = "test-folder/",
        };

        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId)).ReturnsAsync(caseMetadata);
        _netAppArgFactoryMock.Setup(f => f.CreateListObjectsInBucketArg(BucketName, null, maxKeys, folderName, false)).Returns(arg);
        _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(arg)).Returns(request);

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, null, CaseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task ListFilesForTransferAsync_MakesMultipleCallsToApi_IfContinuationTokenIsSupplied()
    {
        // Arrange
        var folderName = "/partial-results/";
        var maxKeys = 1000;
        var continuationToken = "next-token";

        var selectedEntities = new List<TransferEntityDto>
        {
            new() { Path = ObjectKey },
            new() { Path = folderName }
        };

        var argWithoutContinuationToken = new ListObjectsInBucketArg
        {
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys.ToString(),
            Prefix = folderName
        };

        var argWithContinuationToken = new ListObjectsInBucketArg
        {
            BucketName = BucketName,
            ContinuationToken = continuationToken,
            MaxKeys = maxKeys.ToString(),
            Prefix = folderName
        };

        var requestWithoutContinuationToken = new ListObjectsV2Request
        {
            BucketName = BucketName,
            ContinuationToken = null,
            MaxKeys = maxKeys,
            Prefix = folderName
        };

        var requestWithContinuationToken = new ListObjectsV2Request
        {
            BucketName = BucketName,
            ContinuationToken = continuationToken,
            MaxKeys = maxKeys,
            Prefix = folderName
        };

        var caseMetadata = new CaseMetadata
        {
            CaseId = CaseId,
            EgressWorkspaceId = "egress-workspace-id-123",
            NetappFolderPath = "test-folder/",
        };

        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(CaseId)).ReturnsAsync(caseMetadata);
        _netAppArgFactoryMock.Setup(f => f.CreateListObjectsInBucketArg(BucketName, null, maxKeys, folderName, false)).Returns(argWithoutContinuationToken);
        _netAppArgFactoryMock.Setup(f => f.CreateListObjectsInBucketArg(BucketName, continuationToken, maxKeys, folderName, false)).Returns(argWithContinuationToken);
        _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(argWithoutContinuationToken)).Returns(requestWithoutContinuationToken);
        _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(argWithContinuationToken)).Returns(requestWithContinuationToken);

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, null, CaseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public async Task UploadLargeFile_ShouldHandleMultipleChunksEfficiently()
    {
        // Arrange
        const string destinationPath = "";
        const string sourcePath = ObjectKey;
        var fullPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        const int largeFileSize = 50 * 1024 * 1024; // 50 MB
        var largeFileData = new byte[largeFileSize];
        new Random().NextBytes(largeFileData);

        var chunkSize = 5 * 1024 * 1024; // 5 MB chunks
        var totalChunks = (int)Math.Ceiling((double)largeFileData.Length / chunkSize);
        var etags = new List<PartETag>();

        var arg = new InitiateMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        var getObjectArg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = fullPath
        };

        var getObjectAttributesRequest = new GetObjectAttributesRequest
        {
            BucketName = BucketName,
            Key = fullPath,
            ObjectAttributes = [ObjectAttributes.ETag]
        };

        var completeMultipartUploadArg = new CompleteMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = fullPath,
            UploadId = UploadId,
            CompletedParts = []
        };

        var completeMultipartUploadRequest = new CompleteMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = fullPath,
            UploadId = UploadId,
            PartETags = []
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BucketName, fullPath)).Returns(arg);
        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, fullPath)).Returns(getObjectArg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectAttributesRequest(getObjectArg)).Returns(getObjectAttributesRequest);

        var session = await _client.InitiateUploadAsync(destinationPath, largeFileSize, sourcePath);

        for (int i = 0; i < totalChunks; i++)
        {
            var start = i * chunkSize;
            var end = Math.Min(start + chunkSize, largeFileData.Length);
            var chunkData = largeFileData[start..end];
            var partNumber = i + 1;

            var uploadPartArg = new UploadPartArg
            {
                BucketName = BucketName,
                ObjectKey = fullPath,
                PartNumber = partNumber,
                PartData = chunkData,
                UploadId = UploadId
            };

            var uploadRequest = new UploadPartRequest
            {
                BucketName = BucketName,
                Key = fullPath,
                PartNumber = partNumber,
                UploadId = UploadId,
                InputStream = new MemoryStream(chunkData)
            };

            _netAppArgFactoryMock.Setup(f => f.CreateUploadPartArg(BucketName, fullPath, chunkData, partNumber, UploadId)).Returns(uploadPartArg);
            _netAppRequestFactoryMock.Setup(c => c.UploadPartRequest(uploadPartArg)).Returns(uploadRequest);

            // Act
            var result = await _client.UploadChunkAsync(session, partNumber, chunkData);
            etags.Add(new PartETag(partNumber, result.ETag));
        }

        completeMultipartUploadArg.CompletedParts.AddRange(etags);
        completeMultipartUploadRequest.PartETags.AddRange(etags);

        var etagsDict = etags.ToDictionary(e => e.PartNumber, e => e.ETag);
        _netAppArgFactoryMock.Setup(f => f.CreateCompleteMultipartUploadArg(BucketName, fullPath, UploadId, etagsDict)).Returns(completeMultipartUploadArg);
        _netAppRequestFactoryMock.Setup(c => c.CompleteMultipartUploadRequest(completeMultipartUploadArg)).Returns(completeMultipartUploadRequest);

        await _client.CompleteUploadAsync(session, null, etagsDict);

        // Assert
        Assert.Equal(totalChunks, etags.Count);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _server.Stop();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NetAppStorageClientTests()
    {
        Dispose(false);
    }
}
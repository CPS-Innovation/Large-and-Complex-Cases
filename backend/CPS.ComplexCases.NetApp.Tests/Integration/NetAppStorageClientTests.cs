using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.WireMock.Mappings;
using CPS.ComplexCases.NetApp.Wrappers;
using CPS.ComplexCases.WireMock.Core;
using FluentAssertions;
using Moq;
using WireMock.Server;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;

namespace CPS.ComplexCases.NetApp.Tests.Integration;

public class NetAppStorageClientTests : IDisposable
{
    private bool _disposed = false;
    private readonly WireMockServer _server;
    private readonly NetAppClient _netAppClient;
    private readonly NetAppStorageClient _client;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<INetAppRequestFactory> _netAppRequestFactoryMock;
    private const string BucketName = "test-bucket";
    private const string ObjectKey = "test-document.pdf";
    private const string UploadId = "upload-id-49e18525de9c";

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
            ForcePathStyle = true
        };

        var credentials = new Amazon.Runtime.BasicAWSCredentials("fakeAccessKey", "fakeSecretKey");
        var s3Client = new AmazonS3Client(credentials, s3ClientConfig);
        var amazonS3UtilsWrapper = new AmazonS3UtilsWrapper();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _netAppRequestFactoryMock = new Mock<INetAppRequestFactory>();

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

        _netAppClient = new NetAppClient(logger, s3Client, amazonS3UtilsWrapper, _netAppRequestFactoryMock.Object);
        _client = new NetAppStorageClient(_netAppClient, _netAppArgFactoryMock.Object, _netAppOptions);
    }

    [Fact]
    public async Task InitiateUploadAsync_ReturnsUploadSession()
    {
        // Arrange
        var arg = new InitiateMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var getObjectArg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey
        };

        var getObjectAttributesRequest = new GetObjectAttributesRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            ObjectAttributes = [ObjectAttributes.ETag]
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BucketName, ObjectKey)).Returns(arg);
        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, ObjectKey)).Returns(getObjectArg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectAttributesRequest(getObjectArg)).Returns(getObjectAttributesRequest);

        //Act
        var result = await _client.InitiateUploadAsync(ObjectKey, 123);

        // Assert
        result.Should().NotBeNull();
        result.UploadId.Should().Be("upload-id-49e18525de9c");
        result.WorkspaceId.Should().Be(ObjectKey);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhereObjectExists_ThrowsException()
    {
        // Arrange
        const string ExistingObjectKey = "existing-document.pdf";

        var arg = new InitiateMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = ExistingObjectKey
        };

        var getObjectArg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = ExistingObjectKey
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ExistingObjectKey
        };

        var getObjectAttributesRequest = new GetObjectAttributesRequest
        {
            BucketName = BucketName,
            Key = ExistingObjectKey,
            ObjectAttributes = [ObjectAttributes.ETag]
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BucketName, ExistingObjectKey)).Returns(arg);
        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, ExistingObjectKey)).Returns(getObjectArg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectAttributesRequest(getObjectArg)).Returns(getObjectAttributesRequest);

        // Act & Assert
        await Assert.ThrowsAsync<FileExistsException>(() => _client.InitiateUploadAsync(ExistingObjectKey, 123));
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
        result.Should().NotBeNull();
        result.CanRead.Should().BeTrue();

        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task OpenReadStreamAsync_ThrowsIfNullResponse()
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
        await Assert.ThrowsAsync<InvalidOperationException>(() => _client.OpenReadStreamAsync(invalidFilePath));
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
        result.TransferDirection.Should().Be(TransferDirection.EgressToNetApp);
        result.ETag.Should().Be("etag-12345");
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
                new PartETag(1, "etag-12345")
            ]
        };

        var request = new CompleteMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            UploadId = UploadId,
            PartETags = [
                new PartETag(1, "etag-12345")
            ]
        };

        _netAppArgFactoryMock.Setup(f => f.CreateCompleteMultipartUploadArg(BucketName, ObjectKey, UploadId, etags)).Returns(arg);
        _netAppRequestFactoryMock.Setup(c => c.CompleteMultipartUploadRequest(arg)).Returns(request);

        // Act & Assert (should not throw)
        Func<Task> act = () => _client.CompleteUploadAsync(session, null, etags);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UploadLargeFile_ShouldHandleMultipleChunksEfficiently()
    {
        // Arrange
        const int largeFileSize = 50 * 1024 * 1024; // 50 MB
        var largeFileData = new byte[largeFileSize];
        new Random().NextBytes(largeFileData); // Fill with random data

        var chunkSize = 5 * 1024 * 1024; // 5 MB chunks
        var totalChunks = (int)Math.Ceiling((double)largeFileData.Length / chunkSize);
        var etags = new List<PartETag>();

        // Arrange
        var arg = new InitiateMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var getObjectArg = new GetObjectArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        var request = new InitiateMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey
        };

        var getObjectAttributesRequest = new GetObjectAttributesRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            ObjectAttributes = [ObjectAttributes.ETag]
        };

        var completeMultipartUploadArg = new CompleteMultipartUploadArg
        {
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts = []
        };

        var completeMultipartUploadRequest = new CompleteMultipartUploadRequest
        {
            BucketName = BucketName,
            Key = ObjectKey,
            UploadId = UploadId,
            PartETags = []
        };

        _netAppArgFactoryMock.Setup(f => f.CreateInitiateMultipartUploadArg(BucketName, ObjectKey)).Returns(arg);
        _netAppArgFactoryMock.Setup(f => f.CreateGetObjectArg(BucketName, ObjectKey)).Returns(getObjectArg);
        _netAppRequestFactoryMock.Setup(f => f.CreateMultipartUploadRequest(arg)).Returns(request);
        _netAppRequestFactoryMock.Setup(f => f.GetObjectAttributesRequest(getObjectArg)).Returns(getObjectAttributesRequest);

        var session = await _client.InitiateUploadAsync(ObjectKey, largeFileSize);

        for (int i = 0; i < totalChunks; i++)
        {
            var start = i * chunkSize;
            var end = Math.Min(start + chunkSize, largeFileData.Length);
            var chunkData = largeFileData[start..end];

            var uploadPartArg = new UploadPartArg
            {
                BucketName = BucketName,
                ObjectKey = ObjectKey,
                PartNumber = i,
                PartData = chunkData,
                UploadId = UploadId
            };

            var uploadRequest = new UploadPartRequest
            {
                BucketName = BucketName,
                Key = ObjectKey,
                PartNumber = i,
                UploadId = UploadId,
                InputStream = new MemoryStream(chunkData)
            };

            _netAppArgFactoryMock.Setup(f => f.CreateUploadPartArg(BucketName, ObjectKey, chunkData, i, UploadId)).Returns(uploadPartArg);
            _netAppRequestFactoryMock.Setup(c => c.UploadPartRequest(uploadPartArg)).Returns(uploadRequest);

            // Act
            var result = await _client.UploadChunkAsync(session, i, chunkData);
            etags.Add(new PartETag(i, result.ETag));
        }

        completeMultipartUploadArg.CompletedParts.AddRange(etags);
        completeMultipartUploadRequest.PartETags.AddRange(etags);

        var etagsDict = etags.ToDictionary(e => e.PartNumber, e => e.ETag);
        _netAppArgFactoryMock.Setup(f => f.CreateCompleteMultipartUploadArg(BucketName, ObjectKey, UploadId, etagsDict)).Returns(completeMultipartUploadArg);
        _netAppRequestFactoryMock.Setup(c => c.CompleteMultipartUploadRequest(completeMultipartUploadArg)).Returns(completeMultipartUploadRequest);

        // Complete the upload
        await _client.CompleteUploadAsync(session, null, etagsDict);

        // Assert
        etags.Count.Should().Be(totalChunks);
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
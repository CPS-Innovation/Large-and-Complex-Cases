using System.Net;
using Amazon.S3.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit;

public class NetAppStorageClientTests
{
    private readonly Fixture _fixture;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<INetAppS3HttpClient> _netAppS3HttpClientMock;
    private readonly Mock<INetAppS3HttpArgFactory> _netAppS3HttpArgFactoryMock;
    private readonly NetAppStorageClient _client;
    private readonly string BearerToken;
    private readonly string BucketName;
    private readonly string ObjectKey;
    private readonly string UploadId;


    public NetAppStorageClientTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        BearerToken = _fixture.Create<string>();
        BucketName = _fixture.Create<string>();
        ObjectKey = _fixture.Create<string>();
        UploadId = _fixture.Create<string>();

        _netAppClientMock = _fixture.Freeze<Mock<INetAppClient>>();
        _netAppArgFactoryMock = _fixture.Freeze<Mock<INetAppArgFactory>>();
        _caseMetadataServiceMock = _fixture.Freeze<Mock<ICaseMetadataService>>();
        _netAppS3HttpClientMock = _fixture.Freeze<Mock<INetAppS3HttpClient>>();
        _netAppS3HttpArgFactoryMock = _fixture.Freeze<Mock<INetAppS3HttpArgFactory>>();

        _client = new NetAppStorageClient(
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _caseMetadataServiceMock.Object,
            _netAppS3HttpClientMock.Object,
            _netAppS3HttpArgFactoryMock.Object);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhenResponseValid_ReturnsUploadSession()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var sourcePath = _fixture.Create<string>();
        var fullPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = new InitiateMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateInitiateMultipartUploadArg(BearerToken, BucketName, fullPath))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.InitiateMultipartUploadAsync(arg))
            .ReturnsAsync(new InitiateMultipartUploadResponse
            {
                UploadId = _fixture.Create<string>(),
                Key = fullPath,
                ServerSideEncryptionCustomerProvidedKeyMD5 = _fixture.Create<string>()
            });

        // Act
        var result = await _client.InitiateUploadAsync(destinationPath, 123, sourcePath, null, null, null, BearerToken, BucketName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fullPath, result.WorkspaceId);
        Assert.NotNull(result.UploadId);
    }

    [Fact]
    public async Task InitiateUploadAsync_WhenResponseNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var sourcePath = _fixture.Create<string>();
        var fullPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = new InitiateMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = fullPath
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateInitiateMultipartUploadArg(BearerToken, BucketName, fullPath))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.InitiateMultipartUploadAsync(arg))
            .ReturnsAsync((InitiateMultipartUploadResponse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.InitiateUploadAsync(destinationPath, 123, sourcePath, null, null, null, BearerToken, BucketName));
    }

    [Fact]
    public async Task UploadChunkAsync_WhenResultValid_ReturnsUploadChunkResult()
    {
        // Arrange
        var chunkData = _fixture.Create<byte[]>();
        var chunkNumber = _fixture.Create<int>();

        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var arg = new UploadPartArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            PartData = chunkData,
            PartNumber = chunkNumber,
            UploadId = UploadId
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateUploadPartArg(BearerToken, BucketName, ObjectKey, chunkData, chunkNumber, UploadId))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.UploadPartAsync(arg))
            .ReturnsAsync(new UploadPartResponse
            {
                ETag = _fixture.Create<string>(),
                PartNumber = chunkNumber
            });

        // Act
        var result = await _client.UploadChunkAsync(session, chunkNumber, chunkData, null, null, null, BearerToken, BucketName);

        // Assert
        Assert.Equal(TransferDirection.EgressToNetApp, result.TransferDirection);
        Assert.Equal(chunkNumber, result.PartNumber);
        Assert.NotNull(result.ETag);
    }

    [Fact]
    public async Task UploadChunkAsync_WhenResultNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var chunkData = _fixture.Create<byte[]>();
        var chunkNumber = _fixture.Create<int>();

        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var arg = new UploadPartArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            PartData = chunkData,
            PartNumber = chunkNumber,
            UploadId = UploadId
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateUploadPartArg(BearerToken, BucketName, ObjectKey, chunkData, chunkNumber, UploadId))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.UploadPartAsync(arg))
            .ReturnsAsync((UploadPartResponse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.UploadChunkAsync(session, chunkNumber, chunkData, null, null, null, BearerToken, BucketName));
    }

    [Fact]
    public async Task CompleteUploadAsync_WhenVerifyUploadTrue_ReturnsTrue()
    {
        // Arrange
        var resultETag = _fixture.Create<string>();

        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var etags = new Dictionary<int, string> { [1] = _fixture.Create<string>() };

        var completeArg = new CompleteMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts = []
        };

        var getArg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            ETag = resultETag
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateCompleteMultipartUploadArg(BearerToken, BucketName, ObjectKey, UploadId, etags))
            .Returns(completeArg);

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, ObjectKey, resultETag))
            .Returns(getArg);

        _netAppClientMock
            .Setup(c => c.CompleteMultipartUploadAsync(completeArg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompleteMultipartUploadResponse { ETag = resultETag });

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
        };

        _netAppS3HttpArgFactoryMock
            .Setup(f => f.CreateGetHeadObjectArg(BearerToken, BucketName, ObjectKey))
            .Returns(arg);

        _netAppS3HttpClientMock
            .Setup(c => c.GetHeadObjectAsync(arg))
            .ReturnsAsync(new Models.Dto.HeadObjectResponseDto { ETag = resultETag, StatusCode = HttpStatusCode.OK });

        // Act
        var result = await _client.CompleteUploadAsync(session, null, etags, BearerToken, BucketName, ObjectKey);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CompleteUploadAsync_WhenVerifyUploadFalse_ReturnsFalse()
    {
        // Arrange
        var resultETag = _fixture.Create<string>();

        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var etags = new Dictionary<int, string> { [1] = _fixture.Create<string>() };

        var completeArg = new CompleteMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts = []
        };

        var getArg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            ETag = resultETag
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateCompleteMultipartUploadArg(BearerToken, BucketName, ObjectKey, UploadId, etags))
            .Returns(completeArg);

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, ObjectKey, resultETag))
            .Returns(getArg);

        _netAppClientMock
            .Setup(c => c.CompleteMultipartUploadAsync(completeArg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompleteMultipartUploadResponse { ETag = resultETag });

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
        };

        _netAppS3HttpArgFactoryMock
            .Setup(f => f.CreateGetHeadObjectArg(BearerToken, BucketName, ObjectKey))
            .Returns(arg);

        _netAppS3HttpClientMock
            .Setup(c => c.GetHeadObjectAsync(arg))
            .ReturnsAsync(new Models.Dto.HeadObjectResponseDto { StatusCode = HttpStatusCode.NotFound });

        // Act
        var result = await _client.CompleteUploadAsync(session, null, etags, BearerToken, BucketName, ObjectKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CompleteUploadAsync_WhenCompleteReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var session = new UploadSession
        {
            WorkspaceId = ObjectKey,
            UploadId = UploadId
        };

        var etags = new Dictionary<int, string> { [1] = _fixture.Create<string>() };

        var completeArg = new CompleteMultipartUploadArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            UploadId = UploadId,
            CompletedParts = []
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateCompleteMultipartUploadArg(BearerToken, BucketName, ObjectKey, UploadId, etags))
            .Returns(completeArg);

        _netAppClientMock
            .Setup(c => c.CompleteMultipartUploadAsync(completeArg, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompleteMultipartUploadResponse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.CompleteUploadAsync(session, null, etags, BearerToken, BucketName, ObjectKey));
    }

    [Fact]
    public async Task UploadFileAsync_WhenUploadSucceeds_DoesNotThrow()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var relativePath = _fixture.Create<string>();
        var objectKey = Path.Combine(destinationPath, relativePath).Replace('\\', '/');
        var content = _fixture.Create<byte[]>();
        var stream = new MemoryStream(content);

        var arg = new UploadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = objectKey,
            Stream = stream,
            ContentLength = content.Length
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateUploadObjectArg(BearerToken, BucketName, objectKey, stream, content.Length, true))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.UploadObjectAsync(arg))
            .ReturnsAsync(true);

        // Act
        await _client.UploadFileAsync(destinationPath, stream, content.Length, null, relativePath, null, BearerToken, BucketName);

        // Assert
        _netAppClientMock.Verify(c => c.UploadObjectAsync(arg), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WhenUploadFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var destinationPath = _fixture.Create<string>();
        var relativePath = _fixture.Create<string>();
        var objectKey = Path.Combine(destinationPath, relativePath).Replace('\\', '/');
        var content = _fixture.Create<byte[]>();
        var stream = new MemoryStream(content);

        var arg = new UploadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = objectKey,
            Stream = stream,
            ContentLength = content.Length
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateUploadObjectArg(BearerToken, BucketName, objectKey, stream, content.Length, true))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.UploadObjectAsync(arg))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.UploadFileAsync(destinationPath, stream, content.Length, null, relativePath, null, BearerToken, BucketName));
    }

    [Fact]
    public async Task OpenReadStreamAsync_WhenResponseValid_ReturnsStreamAndContentLength()
    {
        // Arrange
        var path = _fixture.Create<string>();
        var content = _fixture.Create<byte[]>();
        var responseStream = new MemoryStream(content);

        var arg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = path
        };

        var response = new GetObjectResponse
        {
            ResponseStream = responseStream,
            ContentLength = content.Length
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, path, null))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.GetObjectAsync(arg))
            .ReturnsAsync(response);

        // Act
        var (stream, contentLength) = await _client.OpenReadStreamAsync(path, null, null, BearerToken, BucketName);

        // Assert
        Assert.Equal(content.Length, contentLength);
        await using (stream)
        {
            using var reader = new StreamReader(stream);
            var resultContent = await reader.ReadToEndAsync();
            Assert.Equal(System.Text.Encoding.UTF8.GetString(content), resultContent);
        }
    }

    [Fact]
    public async Task OpenReadStreamAsync_WhenResponseNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var path = _fixture.Create<string>();

        var arg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = path
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, path, null))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.GetObjectAsync(arg))
            .ReturnsAsync((GetObjectResponse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.OpenReadStreamAsync(path, null, null, BearerToken, BucketName));
    }

    [Fact]
    public async Task VerifyUpload_WhenResponseNull_ReturnsFalse()
    {
        // Arrange
        var eTag = _fixture.Create<string>();

        var arg = new GetObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
            ETag = eTag
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(BearerToken, BucketName, ObjectKey, eTag))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.GetObjectAsync(arg))
            .ReturnsAsync((GetObjectResponse?)null);

        // Act
        var result = await _client.VerifyUpload(BearerToken, BucketName, ObjectKey, eTag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerifyUpload_WhenETagMatches_ReturnsTrue()
    {
        // Arrange
        var eTag = _fixture.Create<string>();

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey
        };

        _netAppS3HttpArgFactoryMock
            .Setup(f => f.CreateGetHeadObjectArg(BearerToken, BucketName, ObjectKey))
            .Returns(arg);

        _netAppS3HttpClientMock
            .Setup(c => c.GetHeadObjectAsync(arg))
            .ReturnsAsync(new Models.Dto.HeadObjectResponseDto { ETag = eTag, StatusCode = HttpStatusCode.OK });

        // Act
        var result = await _client.VerifyUpload(BearerToken, BucketName, ObjectKey, eTag);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyUpload_WhenETagDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var eTag = _fixture.Create<string>();

        var arg = new GetHeadObjectArg
        {
            BearerToken = BearerToken,
            BucketName = BucketName,
            ObjectKey = ObjectKey,
        };

        _netAppS3HttpArgFactoryMock
            .Setup(f => f.CreateGetHeadObjectArg(BearerToken, BucketName, ObjectKey))
            .Returns(arg);

        _netAppS3HttpClientMock
            .Setup(c => c.GetHeadObjectAsync(arg))
            .ReturnsAsync(new Models.Dto.HeadObjectResponseDto { ETag = _fixture.Create<string>() });

        // Act
        var result = await _client.VerifyUpload(BearerToken, BucketName, ObjectKey, eTag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ListFilesForTransferAsync_WhenPathIsFile_ReturnsRelativePath()
    {
        // Arrange
        var caseId = _fixture.Create<int>();
        var netappRoot = "/root";
        var filePath = "/root/folder/file.txt";
        var expectedRelativePath = filePath.RemovePathPrefix(netappRoot);

        var caseMetadata = new CaseMetadata
        {
            CaseId = caseId,
            NetappFolderPath = netappRoot
        };

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(caseId))
            .ReturnsAsync(caseMetadata);

        var selectedEntities = new List<TransferEntityDto>
        {
            new() { Path = filePath }
        };

        // Act
        var result = await _client.ListFilesForTransferAsync(selectedEntities, null, caseId, null, null);

        // Assert
        var file = Assert.Single(result);
        Assert.Equal(filePath, file.SourcePath);
        Assert.Equal(expectedRelativePath, file.RelativePath);
    }
}

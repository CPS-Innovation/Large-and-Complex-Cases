using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Enums;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.NetApp.Wrappers;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit
{
    public class NetAppClientTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ILogger<NetAppClient>> _loggerMock;
        private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
        private readonly Mock<IAmazonS3UtilsWrapper> _amazonS3UtilsWrapperMock;
        private readonly Mock<INetAppRequestFactory> _netAppRequestFactoryMock;
        private readonly Mock<IAmazonS3> _amazonS3Mock;
        private readonly Mock<IS3ClientFactory> _s3ClientFactoryMock;
        private readonly Mock<INetAppS3HttpClient> _netAppS3HttpClientMock;
        private readonly Mock<INetAppS3HttpArgFactory> _netAppS3HttpArgFactoryMock;
        private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
        private readonly NetAppClient _client;
        private const string TestUrl = "https://netapp.com";
        private const string BucketName = "test-bucket";
        private const string RegionName = "eu-west-2";
        private const string BearerToken = "fakeBearerToken";

        public NetAppClientTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _loggerMock = _fixture.Freeze<Mock<ILogger<NetAppClient>>>();
            var options = new NetAppOptions
            {
                Url = TestUrl,
                RegionName = RegionName
            };
            _optionsMock = new Mock<IOptions<NetAppOptions>>();
            _optionsMock.Setup(x => x.Value).Returns(options);
            _amazonS3UtilsWrapperMock = _fixture.Freeze<Mock<IAmazonS3UtilsWrapper>>();
            _netAppRequestFactoryMock = _fixture.Freeze<Mock<INetAppRequestFactory>>();
            _amazonS3Mock = _fixture.Freeze<Mock<IAmazonS3>>();
            _s3ClientFactoryMock = _fixture.Freeze<Mock<IS3ClientFactory>>();
            _s3ClientFactoryMock.Setup(x => x.GetS3ClientAsync(BearerToken)).ReturnsAsync(_amazonS3Mock.Object);
            _netAppS3HttpClientMock = _fixture.Freeze<Mock<INetAppS3HttpClient>>();
            _netAppS3HttpArgFactoryMock = _fixture.Freeze<Mock<INetAppS3HttpArgFactory>>();
            _netAppArgFactoryMock = _fixture.Freeze<Mock<INetAppArgFactory>>();
            _client = new NetAppClient(_loggerMock.Object, _optionsMock.Object, _amazonS3UtilsWrapperMock.Object,
                _netAppRequestFactoryMock.Object, _netAppArgFactoryMock.Object, _s3ClientFactoryMock.Object,
                _netAppS3HttpClientMock.Object, _netAppS3HttpArgFactoryMock.Object);
        }

        [Fact]
        public async Task CreateBucket_WhenBucketExists_ReturnsFalse()
        {
            // Arrange
            var arg = _fixture.Create<CreateBucketArg>();
            var expectedExceptionMessage = $"Bucket with name {arg.BucketName} already exists.";

            _amazonS3UtilsWrapperMock.Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<IAmazonS3>(), arg.BucketName))
                .ReturnsAsync(true);

            // Act
            var result = await _client.CreateBucketAsync(arg);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedExceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task CreateBucket_WhenBucketDoesNotExist_ReturnsTrue()
        {
            // Arrange
            var arg = _fixture.Create<CreateBucketArg>();
            arg.BearerToken = BearerToken;
            _amazonS3UtilsWrapperMock.Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<AmazonS3Client>(), arg.BucketName))
                .ReturnsAsync(false);

            _amazonS3Mock.Setup(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default))
                .ReturnsAsync(new PutBucketResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _client.CreateBucketAsync(arg);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateBucket_WhenExceptionThrown_ReturnsFalse()
        {
            // Arrange
            var arg = _fixture.Create<CreateBucketArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = "Error";

            _amazonS3UtilsWrapperMock.Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<IAmazonS3>(), arg.BucketName))
                .ReturnsAsync(false);

            _amazonS3Mock.Setup(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.CreateBucketAsync(arg);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedExceptionMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task FindBucketAsync_WhenBucketExists_ReturnsBucket()
        {
            // Arrange
            var arg = _fixture.Create<FindBucketArg>();
            arg.BearerToken = BearerToken;
            var bucket = new S3Bucket { BucketName = arg.BucketName };
            var listBucketsResponse = new ListBucketsResponse
            {
                Buckets = [bucket]
            };

            _amazonS3Mock.Setup(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default))
                .ReturnsAsync(listBucketsResponse);

            // Act
            var result = await _client.FindBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(arg.BucketName, result?.BucketName);
        }

        [Fact]
        public async Task FindBucketAsync_WhenBucketDoesNotExist_ReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<FindBucketArg>();
            arg.BearerToken = BearerToken;
            var listBucketsResponse = new ListBucketsResponse
            {
                Buckets = []
            };

            _amazonS3Mock.Setup(x => x.ListBucketsAsync(default))
                .ReturnsAsync(listBucketsResponse);

            // Act
            var result = await _client.FindBucketAsync(arg);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindBucketAsync_WhenExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<FindBucketArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = "Error";

            _amazonS3Mock.Setup(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.FindBucketAsync(arg);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedExceptionMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetObjectAsync_WhenObjectExists_ReturnsGetObjectResponse()
        {
            // Arrange
            var arg = _fixture.Create<GetObjectArg>();
            arg.BearerToken = BearerToken;
            var getObjectResponse = new GetObjectResponse
            {
                BucketName = arg.BucketName,
                Key = arg.ObjectKey,
                ResponseStream = new MemoryStream()
            };

            _amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(getObjectResponse);

            // Act
            var result = await _client.GetObjectAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(arg.BucketName, result?.BucketName);
            Assert.Equal(arg.ObjectKey, result?.Key);
        }

        [Fact]
        public async Task GetObjectAsync_WhenObjectDoesNotExist_ReturnsException()
        {
            // Arrange
            var arg = _fixture.Create<GetObjectArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = $"Failed to get file {arg.ObjectKey} from bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await Record.ExceptionAsync(() => _client.GetObjectAsync(arg));

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedExceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetObjectAsync_WhenExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var arg = _fixture.Create<GetObjectArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = "Error";

            _amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await Record.ExceptionAsync(() => _client.GetObjectAsync(arg));

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedExceptionMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task UploadObjectAsync_WhenUploadSucceeds_ReturnsTrue()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            arg.BearerToken = BearerToken;
            var putObjectResponse = new PutObjectResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(putObjectResponse);

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UploadObjectAsync_WhenUploadFails_ReturnsFalse()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            arg.BearerToken = BearerToken;
            var putObjectResponse = new PutObjectResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(putObjectResponse);

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UploadObjectAsync_WhenExceptionThrown_LogsErrorThrows()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = $"Failed to upload file {arg.ObjectKey} to bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await Record.ExceptionAsync(() => _client.UploadObjectAsync(arg));

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedExceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenObjectsExist_ReturnsListObjectsV2Response()
        {
            // Arrange
            var arg = _fixture.Create<ListObjectsInBucketArg>();
            arg.BearerToken = BearerToken;
            var listObjectsResponse = new ListObjectsV2Response
            {
                S3Objects =
                [
                    new S3Object { Key = "object1" },
                    new S3Object { Key = "object2" }
                ]
            };

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(listObjectsResponse);

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result?.Data.FileData.Count());
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenNoObjectsExist_ReturnsEmptyListObjectsV2Response()
        {
            // Arrange
            var arg = _fixture.Create<ListObjectsInBucketArg>();
            arg.BearerToken = BearerToken;
            var listObjectsResponse = new ListObjectsV2Response
            {
                S3Objects = []
            };

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(listObjectsResponse);

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.FileData);
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<ListObjectsInBucketArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = $"Failed to list objects in bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedExceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenFoldersExist_ReturnsCommonPrefixes()
        {
            // Arrange
            var arg = _fixture.Create<ListFoldersInBucketArg>();
            arg.BearerToken = BearerToken;
            var listObjectsResponse = new ListObjectsV2Response
            {
                CommonPrefixes = ["folder1/", "folder2/"]
            };

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(listObjectsResponse);

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);
            var data = result?.Data.FolderData?.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, data?.Count);
            Assert.NotNull(data);
            Assert.Contains("folder1/", data[0].Path);
            Assert.Contains("folder2/", data[1].Path);
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenNoFoldersExist_ReturnsEmptyList()
        {
            // Arrange
            var arg = _fixture.Create<ListFoldersInBucketArg>();
            arg.BearerToken = BearerToken;
            var listObjectsResponse = new ListObjectsV2Response
            {
                CommonPrefixes = []
            };

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(listObjectsResponse);

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<ListFoldersInBucketArg>();
            arg.BearerToken = BearerToken;
            var expectedExceptionMessage = $"Failed to list objects in bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedExceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task InitiateMultipartUploadAsync_ReturnsResponse_OnSuccess()
        {
            var arg = _fixture.Create<InitiateMultipartUploadArg>();
            arg.BearerToken = BearerToken;
            var response = new InitiateMultipartUploadResponse();

            _amazonS3Mock
                .Setup(s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var result = await _client.InitiateMultipartUploadAsync(arg);

            Assert.Equal(response, result);
        }

        [Fact]
        public async Task InitiateMultipartUploadAsync_ReturnsNull_AndLogs_OnException()
        {
            var arg = new InitiateMultipartUploadArg
            { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = "bucket" };

            _amazonS3Mock.Setup(s =>
                    s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("fail"));

            var result = await _client.InitiateMultipartUploadAsync(arg);

            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v != null && v!.ToString()!.Contains("Failed to initiate multipart upload")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadPartAsync_ReturnsResponse_OnSuccess()
        {
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = [1, 2, 3]
            };
            var response = new UploadPartResponse();

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _client.UploadPartAsync(arg);

            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UploadPartAsync_Throws_OnException_AndLogs()
        {
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = [1, 2, 3]
            };

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("fail"));

            await Assert.ThrowsAsync<AmazonS3Exception>(() => _client.UploadPartAsync(arg));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("Failed to upload part")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadPartAsync_WhenInvalidAccessKeyIdError_RetriesAndSucceeds()
        {
            // Arrange
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };
            var expectedResponse = new UploadPartResponse();
            var callCount = 0;

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return expectedResponse;
                });

            // Act
            var result = await _client.UploadPartAsync(arg);

            // Assert
            Assert.Equal(expectedResponse, result);
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("credentials likely rotated mid-transfer")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadPartAsync_WhenExpiredTokenError_RetriesAndSucceeds()
        {
            // Arrange
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };
            var expectedResponse = new UploadPartResponse();
            var callCount = 0;

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The provided token has expired.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.ExpiredToken };
                    return expectedResponse;
                });

            // Act
            var result = await _client.UploadPartAsync(arg);

            // Assert
            Assert.Equal(expectedResponse, result);
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task UploadPartAsync_WhenCredentialErrorMatchedByMessageFallback_RetriesAndSucceeds()
        {
            // Arrange - 403 Forbidden with the known message text but no standard ErrorCode
            // (guards against NetApp returning a non-standard ErrorCode for this condition)
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };
            var expectedResponse = new UploadPartResponse();
            var callCount = 0;

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden };
                    return expectedResponse;
                });

            // Act
            var result = await _client.UploadPartAsync(arg);

            // Assert
            Assert.Equal(expectedResponse, result);
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task UploadPartAsync_WhenAllRetriesExhausted_ThrowsAndLogs()
        {
            // Arrange
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId });

            // Act & Assert
            await Assert.ThrowsAsync<AmazonS3Exception>(() => _client.UploadPartAsync(arg));

            // 1 initial attempt + 2 retries = 3 total
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("credentials likely rotated mid-transfer")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("after all retry attempts")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadPartAsync_WhenAllRetriesExhaustedWithAccessDenied_ThrowsNetAppAccessDeniedException()
        {
            // Arrange – NetApp keeps returning AccessDenied even after credential refresh;
            // after all 3 attempts the error must surface as NetAppAccessDeniedException, not a raw 500.
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Access Denied")
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.AccessDenied });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.UploadPartAsync(arg));

            // Assert – 1 initial attempt + 2 retries = 3 total, then converted to NetAppAccessDeniedException
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(arg.BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task UploadPartAsync_WhenAccessDeniedError_RetriesAndSucceeds()
        {
            // Arrange - AccessDenied now triggers credential retry (shared Key Vault scenario)
            var arg = new UploadPartArg
            {
                BearerToken = BearerToken,
                UploadId = "1",
                ObjectKey = "file.txt",
                BucketName = "bucket",
                PartNumber = 1,
                PartData = new byte[] { 1, 2, 3 }
            };
            var expectedResponse = new UploadPartResponse();
            var callCount = 0;

            _amazonS3Mock.Setup(s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.AccessDenied };
                    return expectedResponse;
                });

            // Act
            var result = await _client.UploadPartAsync(arg);

            // Assert
            Assert.Equal(expectedResponse, result);
            _amazonS3Mock.Verify(
                s => s.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task CompleteMultipartUploadAsync_ReturnsResponse_OnSuccess()
        {
            var arg = new CompleteMultipartUploadArg
            {
                BearerToken = BearerToken,
                ObjectKey = "file.txt",
                BucketName = "bucket",
                UploadId = "uploadid",
                CompletedParts = []
            };
            var response = new CompleteMultipartUploadResponse();

            _amazonS3Mock
                .Setup(s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var result = await _client.CompleteMultipartUploadAsync(arg);

            Assert.Equal(response, result);
        }

        [Fact]
        public async Task CompleteMultipartUploadAsync_Throws_OnNonTransientException_AndDoesNotRetry()
        {
            var arg = new CompleteMultipartUploadArg
            {
                BearerToken = BearerToken,
                ObjectKey = "file.txt",
                BucketName = "bucket",
                UploadId = "uploadid",
                CompletedParts = []
            };

            _amazonS3Mock.Setup(s =>
                    s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception(S3ErrorCodes.AccessDenied) { StatusCode = HttpStatusCode.Forbidden });

            await Assert.ThrowsAsync<AmazonS3Exception>(() => _client.CompleteMultipartUploadAsync(arg));
            _amazonS3Mock.Verify(
                s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("after all retry attempts")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CompleteMultipartUploadAsync_WhenAllRetriesExhaustedWithAccessDenied_ThrowsNetAppAccessDeniedException()
        {
            // Arrange – NetApp keeps returning AccessDenied even after credential refresh;
            // after all retries the error must surface as NetAppAccessDeniedException, not a raw 500.
            var arg = new CompleteMultipartUploadArg
            {
                BearerToken = BearerToken,
                ObjectKey = "file.txt",
                BucketName = "bucket",
                UploadId = "uploadid",
                CompletedParts = []
            };

            _amazonS3Mock
                .Setup(s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.AccessDenied });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.CompleteMultipartUploadAsync(arg));

            // Assert – 1 initial attempt + 5 retries = 6 total, then converted to NetAppAccessDeniedException
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(arg.BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _amazonS3Mock.Verify(
                s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(6));
        }

        [Fact]
        public async Task CompleteMultipartUploadAsync_RetriesOnTransient500_AndSucceeds()
        {
            var arg = new CompleteMultipartUploadArg
            {
                BearerToken = BearerToken,
                ObjectKey = "file.txt",
                BucketName = "bucket",
                UploadId = "uploadid",
                CompletedParts = []
            };
            var expectedResponse = new CompleteMultipartUploadResponse();

            var callCount = 0;
            _amazonS3Mock
                .Setup(s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("We encountered an internal error. Please try again.")
                        { StatusCode = HttpStatusCode.InternalServerError };
                    return expectedResponse;
                });

            var result = await _client.CompleteMultipartUploadAsync(arg);

            Assert.Equal(expectedResponse, result);
            _amazonS3Mock.Verify(
                s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v != null && v!.ToString()!.Contains("CompleteMultipartUpload retry attempt")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CompleteMultipartUploadAsync_ExhaustsRetries_AndThrows()
        {
            var arg = new CompleteMultipartUploadArg
            {
                BearerToken = BearerToken,
                ObjectKey = "file.txt",
                BucketName = "bucket",
                UploadId = "uploadid",
                CompletedParts = []
            };

            _amazonS3Mock
                .Setup(s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("We encountered an internal error. Please try again.")
                { StatusCode = HttpStatusCode.InternalServerError });

            await Assert.ThrowsAsync<AmazonS3Exception>(() => _client.CompleteMultipartUploadAsync(arg));
            _amazonS3Mock.Verify(
                s => s.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(6)); // 1 initial + 5 retries
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v != null && v!.ToString()!.Contains("CompleteMultipartUpload retry attempt")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(5));
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("after all retry attempts")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DoesObjectExistAsync_ReturnsTrue_OnSuccess()
        {
            var arg = new GetObjectArg { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = "bucket" };
            var response = new HeadObjectResponseDto { StatusCode = HttpStatusCode.OK };

            _netAppS3HttpClientMock.Setup(s => s.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()))
                .ReturnsAsync(response);

            var result = await _client.DoesObjectExistAsync(arg);

            Assert.True(result);
        }

        [Fact]
        public async Task DoesObjectExistAsync_ReturnsFalse_OnNotFound()
        {
            var arg = new GetObjectArg { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = "bucket" };
            var response = new HeadObjectResponseDto { StatusCode = HttpStatusCode.NotFound };

            _netAppS3HttpClientMock.Setup(s => s.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()))
                .ReturnsAsync(response);

            var result = await _client.DoesObjectExistAsync(arg);

            Assert.False(result);
        }

        [Fact]
        public async Task DoesObjectExistAsync_Throws_AndLogs_OnForbiddenException()
        {
            var arg = new GetObjectArg { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = "bucket" };
            var ex = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            _netAppS3HttpClientMock.Setup(s => s.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()))
                .ThrowsAsync(ex);

            await Assert.ThrowsAsync<HttpRequestException>(() => _client.DoesObjectExistAsync(arg));

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("HTTP request failed while getting head object metadata")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DoesObjectExistAsync_Throws_AndLogs_OnOtherException()
        {
            var arg = new GetObjectArg { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = "bucket" };
            var ex = new Exception("fail") { };

            _netAppS3HttpClientMock.Setup(s => s.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()))
                .ThrowsAsync(ex);

            await Assert.ThrowsAsync<Exception>(() => _client.DoesObjectExistAsync(arg));

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("Failed to get head object metadata")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateFolderAsync_ShouldReturnTrue_WhenPutFolderSucceeds()
        {
            // Arrange
            var arg = new CreateFolderArg
            {
                BearerToken = BearerToken,
                BucketName = "bucket",
                FolderKey = "test-folder"
            };

            _netAppS3HttpClientMock
                .Setup(x => x.PutFolderAsync(arg))
                .ReturnsAsync(true);

            // Act
            var result = await _client.CreateFolderAsync(arg);

            // Assert
            Assert.True(result);
            _netAppS3HttpClientMock.Verify(x => x.PutFolderAsync(arg), Times.Once);
            _amazonS3Mock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Never);
            _amazonS3Mock.Verify(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default), Times.Never);
        }

        [Fact]
        public async Task CreateFolderAsync_ShouldReturnFalse_WhenPutFolderFails()
        {
            // Arrange
            var arg = new CreateFolderArg
            {
                BearerToken = BearerToken,
                BucketName = "bucket",
                FolderKey = "test-folder"
            };

            _netAppS3HttpClientMock
                .Setup(x => x.PutFolderAsync(arg))
                .ReturnsAsync(false);

            // Act
            var result = await _client.CreateFolderAsync(arg);

            // Assert
            Assert.False(result);
            _netAppS3HttpClientMock.Verify(x => x.PutFolderAsync(arg), Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenPathHasExtension_DeletesFileSuccessfully()
        {
            // Arrange
            var filePath = "statement/witness.pdf";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = filePath,
                OperationName = "test-operation",
                IsFolder = false
            };

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = arg.BucketName,
                Key = filePath
            };

            SetupFileExistsCheck(filePath, exists: true);

            _netAppRequestFactoryMock
                .Setup(x => x.DeleteObjectRequest(arg))
                .Returns(deleteRequest);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectAsync(deleteRequest, default))
                .ReturnsAsync(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.KeysDeleted);
            _amazonS3Mock.Verify(x => x.DeleteObjectAsync(deleteRequest, default), Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenFileDoesNotExist_ReturnsSuccessWithZeroKeysDeleted()
        {
            // Arrange
            var filePath = "statement/missing.pdf";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = filePath,
                OperationName = "test-operation",
                IsFolder = false
            };

            SetupFileExistsCheck(filePath, exists: false);

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.KeysDeleted);
            _amazonS3Mock.Verify(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default), Times.Never);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenFolderDeletionHasFailures_ReturnsFailureMessage()
        {
            // Arrange
            var folderPath = "statements/witnesses";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            var filesToDelete = new List<string>
            {
                "statements/witnesses/file1.pdf"
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = filesToDelete.Select(f => new S3Object { Key = f, Size = 100 }).ToList(),
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [],
                    DeleteErrors = filesToDelete.Select(f => new DeleteError { Key = f, Code = "AccessDenied", Message = "Access denied" }).ToList()
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Deletion failed for", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenS3ExceptionThrown_RethrowsException()
        {
            // Arrange
            var filePath = "statements/witness.pdf";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = filePath,
                OperationName = "test-operation"
            };

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = arg.BucketName,
                Key = filePath
            };

            SetupFileExistsCheck(filePath, exists: true);

            _netAppRequestFactoryMock
                .Setup(x => x.DeleteObjectRequest(arg))
                .Returns(deleteRequest);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectAsync(deleteRequest, default))
                .ThrowsAsync(new AmazonS3Exception(S3ErrorCodes.AccessDenied));

            // Act & Assert
            await Assert.ThrowsAsync<AmazonS3Exception>(() => _client.DeleteFileOrFolderAsync(arg));

            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Theory]
        [InlineData("file.pdf")]
        [InlineData("document.docx")]
        [InlineData("image.png")]
        [InlineData("archive.zip")]
        public async Task DeleteFileOrFolderAsync_WithVariousFileExtensions_IdentifiesAsFile(string filePath)
        {
            // Arrange
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = filePath,
                OperationName = "test-operation"
            };

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = arg.BucketName,
                Key = filePath
            };

            SetupFileExistsCheck(filePath, exists: true);

            _netAppRequestFactoryMock
                .Setup(x => x.DeleteObjectRequest(arg))
                .Returns(deleteRequest);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectAsync(deleteRequest, default))
                .ReturnsAsync(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.True(result.Success);
            _amazonS3Mock.Verify(x => x.DeleteObjectAsync(deleteRequest, default), Times.Once);
        }

        [Theory]
        [InlineData("folder")]
        [InlineData("path/to/folder")]
        [InlineData("documents")]
        public async Task DeleteFileOrFolderAsync_WithoutExtension_IdentifiesAsFolder(string folderPath)
        {
            // Arrange — bare path is normalised to "folderPath/" inside the client.
            var normalised = folderPath + "/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            // Listing returns nothing → client probes HEAD on the marker key.
            SetupFileExistsCheck(normalised, exists: true);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [new DeletedObject { Key = normalised }],
                    DeleteErrors = []
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert — HEAD probe is made once for the marker key; folder is found and deleted.
            Assert.True(result.Success);
            Assert.True(result.WasFound);
            _netAppS3HttpClientMock.Verify(
                x => x.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenPathAlreadyHasTrailingSlash_DeletesSuccessfully()
        {
            // Arrange — path already ends with "/" (the standard folder-marker convention).
            var folderPath = "capricorn/DemoEditedV3/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            // Listing returns nothing → client probes HEAD on the marker key.
            SetupFileExistsCheck(folderPath, exists: true);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [new DeletedObject { Key = folderPath }],
                    DeleteErrors = []
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert — HEAD probe made once; folder found and deleted successfully.
            Assert.True(result.Success);
            Assert.True(result.WasFound);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
            _netAppS3HttpClientMock.Verify(
                x => x.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenFolderDoesNotExist_ReturnsWasFoundFalse()
        {
            // Arrange — folder produces an empty listing and the marker key returns 404.
            // Client must return WasFound = false without calling DeleteObjectsAsync,
            // mirroring the file deletion path.
            var folderPath = "statements/missing-folder/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            // Marker key does not exist.
            SetupFileExistsCheck(folderPath, exists: false);

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert — HEAD probed once; folder not found; deletion skipped.
            Assert.True(result.Success);
            Assert.False(result.WasFound);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Never);
            _netAppS3HttpClientMock.Verify(
                x => x.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenEmptyFolderAppearsOnLaterPageOfParentListing_ReturnsWasFoundTrueAndDeletes()
        {
            // Arrange — simulates an SMB/NFS-created folder with no marker key.
            // HEAD on the marker returns 404, and the folder appears on page 2 of
            // the parent prefix listing rather than page 1.
            var folderPath = "cases/123/Uploads/OldEvidence/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            // Call 1: listing the folder's own contents (ListAllObjectKeysForDeletionAsync) — empty.
            // Call 2: parent listing page 1 — truncated, folder not yet visible.
            // Call 3: parent listing page 2 — folder appears in CommonPrefixes.
            _amazonS3Mock
                .SetupSequence(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                })
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = ["cases/123/Uploads/SomeOtherFolder/"],
                    IsTruncated = true,
                    NextContinuationToken = "page2token"
                })
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [folderPath],
                    IsTruncated = false
                });

            // Marker key does not exist (SMB/NFS-created folder has no S3 marker).
            SetupFileExistsCheck(folderPath, exists: false);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [new DeletedObject { Key = folderPath }],
                    DeleteErrors = []
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert — folder found via page-2 parent listing; deletion proceeds.
            Assert.True(result.Success);
            Assert.True(result.WasFound);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenParentListingFailsTransiently_ThrowsInvalidOperationException()
        {
            // Arrange — simulates a transient S3 failure while listing the parent prefix
            // during the fallback existence check. This must propagate as an exception
            // rather than silently returning WasFound = false, to avoid masking failures
            // as a normal no-op delete.
            var folderPath = "cases/123/Uploads/OldEvidence/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            // Call 1: listing the folder's own contents — empty (triggers the existence check).
            // Call 2: parent listing fails with a non-credential S3 error, causing
            //         ListObjectsInBucketAsync to return null.
            _amazonS3Mock
                .SetupSequence(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                })
                .ThrowsAsync(new AmazonS3Exception("Simulated transient S3 failure"));

            // Marker key does not exist.
            SetupFileExistsCheck(folderPath, exists: false);

            // Act & Assert — the transient listing failure must surface as an exception,
            // not silently map to WasFound = false.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.DeleteFileOrFolderAsync(arg));

            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Never);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenDeleteObjectsAsync_WithOkStatusAndNoErrors_ReturnsSuccessMessage()
        {
            // Arrange
            var folderPath = "witnesses/statements";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            var filesToDelete = new List<string>
            {
                "witnesses/statements/statement1.pdf",
                "witnesses/statements/statement2.pdf",
                "witnesses/statements/"
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = filesToDelete.Where(f => !f.EndsWith('/'))
                        .Select(f => new S3Object { Key = f, Size = 100 }).ToList(),
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = filesToDelete.Select(f => new DeletedObject { Key = f }).ToList(),
                    DeleteErrors = []
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.True(result.Success);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task
            DeleteFileOrFolderAsync_WhenDeleteObjectsAsync_WithPartialFailures_ReturnsPartialFailureMessage()
        {
            // Arrange
            var folderPath = "witnesses/statements";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            var successfulDeletions = 2;

            var filesToDelete = new List<string>
            {
                "witnesses/statements/statement1.pdf",
                "witnesses/statements/statement2.pdf",
                "witnesses/statements/"
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = filesToDelete.Where(f => !f.EndsWith('/'))
                        .Select(f => new S3Object { Key = f, Size = 100 }).ToList(),
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            var deletedObjects = filesToDelete.Take(successfulDeletions).Select(f => new DeletedObject { Key = f })
                .ToList();
            var deleteErrors = new List<DeleteError>
            {
                new DeleteError { Key = filesToDelete.Last(), Code = S3ErrorCodes.AccessDenied, Message = S3ErrorCodes.AccessDenied }
            };

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = deletedObjects,
                    DeleteErrors = deleteErrors
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(successfulDeletions, result.KeysDeleted);
            Assert.Contains("Deletion failed for", result.ErrorMessage);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenDeleteObjectsAsync_WithAllFailures_ReturnsFailureMessage()
        {
            // Arrange
            var folderPath = "witnesses/statements";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            var filesToDelete = new List<string>
            {
                "witnesses/statements/statement1.pdf",
                "witnesses/statements/statement2.pdf",
                "witnesses/statements/"
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = filesToDelete.Where(f => !f.EndsWith('/'))
                        .Select(f => new S3Object { Key = f, Size = 100 }).ToList(),
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            var deleteErrors = filesToDelete.Select(f => new DeleteError
            { Key = f, Code = S3ErrorCodes.AccessDenied, Message = S3ErrorCodes.AccessDenied }).ToList();

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [],
                    DeleteErrors = deleteErrors
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.KeysDeleted);
            Assert.Contains("Deletion failed for", result.ErrorMessage);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(deleteErrors.Count));
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenDeleteObjectsAsync_WithEmptyFolder_ReturnsSuccessMessage()
        {
            // Arrange — folder exists (marker returns 200) but contains no files.
            var folderPath = "witnesses/empty";
            var normalised = folderPath + "/";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [],
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            // Listing returns nothing → client probes HEAD; marker exists.
            SetupFileExistsCheck(normalised, exists: true);

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [new DeletedObject { Key = normalised }],
                    DeleteErrors = []
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert — HEAD probed once; folder found and marker deleted.
            Assert.True(result.Success);
            Assert.True(result.WasFound);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
            _netAppS3HttpClientMock.Verify(
                x => x.GetHeadObjectAsync(It.IsAny<GetHeadObjectArg>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFileOrFolderAsync_WhenDeleteObjectsAsync_LogsErrorsForFailedDeletions()
        {
            // Arrange
            var folderPath = "witnesses/statements";
            var arg = new DeleteFileOrFolderArg
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                Path = folderPath,
                OperationName = "test-operation",
                IsFolder = true
            };

            var filesToDelete = new List<string>
            {
                "witnesses/statements/statement1.pdf",
                "witnesses/statements/"
            };

            var errorKey = "witnesses/statements/statement1.pdf";
            var errorCode = "AccessDenied";
            var errorMessage = "User does not have permission";

            _netAppRequestFactoryMock
                .Setup(x => x.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request { BucketName = BucketName });

            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = [new S3Object { Key = errorKey, Size = 100 }],
                    CommonPrefixes = [],
                    IsTruncated = false
                });

            var deleteErrors = new List<DeleteError>
            {
                new DeleteError { Key = errorKey, Code = errorCode, Message = errorMessage }
            };

            _amazonS3Mock
                .Setup(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                .ReturnsAsync(new DeleteObjectsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    DeletedObjects = [new DeletedObject { Key = folderPath }],
                    DeleteErrors = deleteErrors
                });

            // Act
            var result = await _client.DeleteFileOrFolderAsync(arg);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains(errorKey) && v.ToString()!.Contains(errorCode) &&
                        v.ToString()!.Contains(errorMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateFolderAsync_ShouldLogWarning_WhenPutFolderFails()
        {
            // Arrange
            var arg = new CreateFolderArg { BearerToken = BearerToken, BucketName = "bucket", FolderKey = "folder" };

            _netAppS3HttpClientMock
                .Setup(x => x.PutFolderAsync(arg))
                .ReturnsAsync(false);

            // Act
            await _client.CreateFolderAsync(arg);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("folder") || v.ToString()!.Contains("bucket")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateFolderAsync_WhenCredentialError_RetriesAndSucceeds()
        {
            // Arrange
            var arg = new CreateFolderArg { BearerToken = BearerToken, BucketName = "bucket", FolderKey = "test-folder" };
            var callCount = 0;

            _netAppS3HttpClientMock
                .Setup(x => x.PutFolderAsync(arg))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new S3CredentialException("HTTP 403 received — credentials expired.");
                    return true;
                });

            // Act
            var result = await _client.CreateFolderAsync(arg);

            // Assert
            Assert.True(result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _netAppS3HttpClientMock.Verify(x => x.PutFolderAsync(arg), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateFolderAsync_WhenCredentialRetryAlsoFails_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = new CreateFolderArg { BearerToken = BearerToken, BucketName = "bucket", FolderKey = "test-folder" };

            _netAppS3HttpClientMock
                .Setup(x => x.PutFolderAsync(arg))
                .ThrowsAsync(new S3CredentialException("HTTP 403 received — credentials expired."));

            // Act & Assert
            await Assert.ThrowsAsync<NetAppAccessDeniedException>(() => _client.CreateFolderAsync(arg));

            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _netAppS3HttpClientMock.Verify(x => x.PutFolderAsync(arg), Times.Exactly(2));
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenCredentialError_RetriesAndSucceeds()
        {
            // Arrange
            var arg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "bucket" };
            var callCount = 0;

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return new ListObjectsV2Response { S3Objects = [] };
                });

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadObjectAsync_WhenCredentialError_RetriesAndSucceeds()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            arg.BearerToken = BearerToken;

            var callCount = 0;
            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.AccessDenied };
                    return new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK };
                });

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            Assert.True(result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBucketAsync_WhenCredentialError_InvalidatesClientAndRetriesSuccessfully()
        {
            // Arrange
            var arg = _fixture.Create<CreateBucketArg>();
            arg.BearerToken = BearerToken;

            _amazonS3UtilsWrapperMock
                .Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<IAmazonS3>(), arg.BucketName))
                .ReturnsAsync(false);

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return new PutBucketResponse { HttpStatusCode = HttpStatusCode.OK };
                });

            // Act
            var result = await _client.CreateBucketAsync(arg);

            // Assert
            Assert.True(result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateBucketAsync_WhenAccessDeniedOnRetry_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = _fixture.Create<CreateBucketArg>();
            arg.BearerToken = BearerToken;

            _amazonS3UtilsWrapperMock
                .Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<IAmazonS3>(), arg.BucketName))
                .ReturnsAsync(false);

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception("Access denied")
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.CreateBucketAsync(arg));

            // Assert
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(arg.BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task ListBucketsAsync_WhenCredentialError_InvalidatesClientAndRetriesSuccessfully()
        {
            // Arrange
            var arg = new ListBucketsArg { BearerToken = BearerToken, BucketName = BucketName };
            var expectedBucket = new S3Bucket { BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return new ListBucketsResponse { Buckets = [expectedBucket] };
                });

            // Act
            var result = await _client.ListBucketsAsync(arg);

            // Assert
            Assert.Single(result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task ListBucketsAsync_WhenAccessDeniedOnRetry_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = new ListBucketsArg { BearerToken = BearerToken, BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.ListBucketsAsync(arg));

            // Assert
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.ListBucketsAsync(It.IsAny<ListBucketsRequest>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenAccessDeniedOnRetry_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.ListObjectsInBucketAsync(arg));

            // Assert
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenCredentialError_InvalidatesClientAndRetriesSuccessfully()
        {
            // Arrange
            var arg = new ListFoldersInBucketArg { BearerToken = BearerToken, BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return new ListObjectsV2Response { CommonPrefixes = ["folder1/"] };
                });

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data.FolderData);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenAccessDeniedOnRetry_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = new ListFoldersInBucketArg { BearerToken = BearerToken, BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.ListFoldersInBucketAsync(arg));

            // Assert
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task InitiateMultipartUploadAsync_WhenCredentialError_InvalidatesClientAndRetriesSuccessfully()
        {
            // Arrange
            var arg = new InitiateMultipartUploadArg
            { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = BucketName };
            var expectedResponse = new InitiateMultipartUploadResponse();

            var callCount = 0;
            _amazonS3Mock
                .Setup(s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return expectedResponse;
                });

            // Act
            var result = await _client.InitiateMultipartUploadAsync(arg);

            // Assert
            Assert.Equal(expectedResponse, result);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(
                s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task InitiateMultipartUploadAsync_WhenAccessDeniedOnRetry_ThrowsNetAppAccessDeniedException()
        {
            // Arrange
            var arg = new InitiateMultipartUploadArg
            { BearerToken = BearerToken, ObjectKey = "file.txt", BucketName = BucketName };

            var callCount = 0;
            _amazonS3Mock
                .Setup(s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception(S3ErrorCodes.AccessDenied)
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.InitiateMultipartUploadAsync(arg));

            // Assert
            Assert.IsType<NetAppAccessDeniedException>(ex);
            Assert.Equal(BucketName, ((NetAppAccessDeniedException)ex).BucketName);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(
                s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_ReturnsFilesAndFolders()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/doc-file.txt", "test-operation/document.pdf"],
                folderPrefixes: ["test-operation/docs/"]);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/doc", true))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result!.Data.Count());
            Assert.Contains(result.Data, x => x.Key == "test-operation/doc-file.txt" && x.Type == "File");
            Assert.Contains(result.Data, x => x.Key == "test-operation/document.pdf" && x.Type == "File");
            Assert.Contains(result.Data, x => x.Key == "test-operation/docs/" && x.Type == "Folder");
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_TruncatedTrue_WhenNextContinuationTokenPresent()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/doc1.txt"],
                nextContinuationToken: "token-page2");

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/doc", true))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_TruncatedFalse_WhenNoNextContinuationToken()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(fileKeys: ["test-operation/doc1.txt"]);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/doc", true))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.False(result!.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_NoQuery_SearchesOperationNameOnly()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: null);
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(fileKeys: ["test-operation/file.txt"]);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/", true))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Data);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_ReturnsEmptyResult_WhenListReturnsNull()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/doc", true))
                .Returns(listArg);
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request());
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ThrowsAsync(new AmazonS3Exception("error")); // causes ListObjectsInBucketAsync to return null

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.Data);
            Assert.False(result.Truncated);
            Assert.Equal(0, result.TotalScanned);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_FolderDataWithNullPath_IsExcluded()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = new ListObjectsV2Response
            {
                S3Objects = [new S3Object { Key = "test-operation/doc.txt" }],
                CommonPrefixes = [null!, ""],  // null and empty paths should be filtered out
                KeyCount = 1
            };

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation/doc", true))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Data);
            Assert.All(result.Data, x => Assert.Equal("File", x.Type));
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_UsesBothDelimiterVariants()
        {
            // Arrange — SearchPrefixAsync makes two calls: one with IncludeDelimiter=false
            // (deep file results) and one with IncludeDelimiter=true (immediate folder structure).
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(fileKeys: ["test-operation/doc.txt"]);

            var capturedArgs = new List<ListObjectsInBucketArg>();
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Callback<ListObjectsInBucketArg>(capturedArgs.Add)
                .Returns(s3Request);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request, default))
                .ReturnsAsync(s3Response);

            // Act
            await _client.SearchObjectsInBucketAsync(arg);

            // Assert — exactly two calls are made, one without delimiter and one with
            Assert.Equal(2, capturedArgs.Count);
            Assert.Contains(capturedArgs, a => !a.IncludeDelimiter);
            Assert.Contains(capturedArgs, a => a.IncludeDelimiter);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_DoesNotAppendTrailingSlashToSearchTerm()
        {
            // Arrange — the prefix passed to S3 must be "test-operation/<query>" with no
            // trailing slash appended to the query, so prefix-matching is open-ended.
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(fileKeys: ["test-operation/doc.txt"]);

            ListObjectsInBucketArg? capturedArg = null;
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Callback<ListObjectsInBucketArg>(a => capturedArg = a)
                .Returns(s3Request);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request, default))
                .ReturnsAsync(s3Response);

            // Act
            await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(capturedArg);
            Assert.Equal("test-operation/doc", capturedArg!.Prefix);
            Assert.False(capturedArg.Prefix!.EndsWith('/'), "Prefix must not end with '/' when a query is supplied.");
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_ReturnsDeepMatchesAcrossNestedPaths()
        {
            // Arrange — because IncludeDelimiter = false, S3 returns all objects under the
            // prefix recursively. Verify that files at multiple nesting levels are all mapped.
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys:
                [
                    "test-operation/doc.txt",
                    "test-operation/documents/report.pdf",
                    "test-operation/documents/2025/review.docx"
                ]);

            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(s3Request);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request, default))
                .ReturnsAsync(s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert — all three deeply-nested files are returned as individual results
            Assert.NotNull(result);
            Assert.Equal(3, result!.Data.Count());
            Assert.All(result.Data, x => Assert.Equal("File", x.Type));
            Assert.Contains(result.Data, x => x.Key == "test-operation/doc.txt");
            Assert.Contains(result.Data, x => x.Key == "test-operation/documents/report.pdf");
            Assert.Contains(result.Data, x => x.Key == "test-operation/documents/2025/review.docx");
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_TrimsToMaxResults_AndSetsTruncated_WhenMergedCountExceedsLimit()
        {
            // Arrange — no-delimiter call returns 2 files; delimiter call returns 1 additional folder.
            // MaxResults = 2, so the merged list of 3 must be trimmed to 2 and Truncated must be true.
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc", maxResults: 2);

            var noDelimRequest = new ListObjectsV2Request { BucketName = BucketName, Delimiter = null };
            var delimRequest = new ListObjectsV2Request { BucketName = BucketName, Delimiter = "/" };

            var noDelimResponse = CreateListObjectsV2Response(
                fileKeys: ["test-operation/doc-a.txt", "test-operation/doc-b.txt"]);
            var delimResponse = CreateListObjectsV2Response(
                folderPrefixes: ["test-operation/documents/"]);

            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.Is<ListObjectsInBucketArg>(a => !a.IncludeDelimiter)))
                .Returns(noDelimRequest);
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.Is<ListObjectsInBucketArg>(a => a.IncludeDelimiter)))
                .Returns(delimRequest);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(noDelimRequest, default))
                .ReturnsAsync(noDelimResponse);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(delimRequest, default))
                .ReturnsAsync(delimResponse);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.True(result.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_PrefixMode_DoesNotTruncate_WhenMergedCountIsWithinLimit()
        {
            // Arrange — no-delimiter call returns 1 file; delimiter call returns 1 folder.
            // MaxResults = 5, so the combined 2 items fit and Truncated must be false.
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc", maxResults: 5);

            var noDelimRequest = new ListObjectsV2Request { BucketName = BucketName, Delimiter = null };
            var delimRequest = new ListObjectsV2Request { BucketName = BucketName, Delimiter = "/" };

            var noDelimResponse = CreateListObjectsV2Response(
                fileKeys: ["test-operation/doc-a.txt"]);
            var delimResponse = CreateListObjectsV2Response(
                folderPrefixes: ["test-operation/documents/"]);

            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.Is<ListObjectsInBucketArg>(a => !a.IncludeDelimiter)))
                .Returns(noDelimRequest);
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.Is<ListObjectsInBucketArg>(a => a.IncludeDelimiter)))
                .Returns(delimRequest);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(noDelimRequest, default))
                .ReturnsAsync(noDelimResponse);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(delimRequest, default))
                .ReturnsAsync(delimResponse);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.False(result.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_ReturnsMatchingItems_SinglePage()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Substring, query: "statement");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/witness-statement.txt", "test-operation/report.txt", "test-operation/victim-statement.pdf"],
                keyCount: 3);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation", false))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.All(result.Data, x => Assert.Contains("statement", x.Key));
            Assert.Equal(3, result.TotalScanned);
            Assert.False(result.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_IsCaseInsensitive()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Substring, query: "STATEMENT");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/witness-statement.txt", "test-operation/report.txt"],
                keyCount: 2);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation", false))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Data);
            Assert.Equal("test-operation/witness-statement.txt", result.Data.First().Key);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_ReturnsOnlyMatchingFolder_WhenQueryMatchesParentSegmentNotBasename()
        {
            var arg = CreateSearchArg(SearchModes.Substring, query: "test3");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/evidence/test3/Report.pdf"],
                keyCount: 1);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation", false))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            var result = await _client.SearchObjectsInBucketAsync(arg);

            Assert.NotNull(result);
            Assert.Single(result!.Data);
            var folder = result.Data.Single();
            Assert.Equal(S3SearchResultTypes.Folder, folder.Type);
            Assert.Equal("test-operation/evidence/test3/", folder.Key);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_DetectsFolderFromTrailingSlashKey_WhenDelimiterFalse()
        {
            var arg = CreateSearchArg(SearchModes.Substring, query: "evidence");
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys:
                [
                    "test-operation/evidence/",          // virtual folder marker
                    "test-operation/evidence-report.txt" // regular file
                ],
                keyCount: 2);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation", false))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());

            var folder = result.Data.Single(x => x.Key == "test-operation/evidence/");
            Assert.Equal(S3SearchResultTypes.Folder, folder.Type);
            Assert.Null(folder.LastModified);
            Assert.Null(folder.Size);

            var file = result.Data.Single(x => x.Key == "test-operation/evidence-report.txt");
            Assert.Equal(S3SearchResultTypes.File, file.Type);
            Assert.NotNull(file.LastModified);
            Assert.NotNull(file.Size);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_PaginatesUntilTokenIsNull()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Substring, query: "file", maxResults: 2);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page1" };
            var listArg2 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page2" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "page1" };
            var s3Request2 = new ListObjectsV2Request { BucketName = "page2" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false))
                .Returns(listArg1);
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", 2, "test-operation", false))
                .Returns(listArg2);

            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg2)).Returns(s3Request2);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/file1.txt", "test-operation/other.txt"],
                    nextContinuationToken: "token-page2",
                    keyCount: 2));
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request2, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/file2.txt", "test-operation/another.txt"],
                    keyCount: 2));

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.All(result.Data, x => Assert.Contains("file", x.Key));
            Assert.Equal(4, result.TotalScanned);
            Assert.False(result.Truncated);
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false), Times.Once);
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", 2, "test-operation", false), Times.Once);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_StopsAndSetsTruncated_WhenScanLimitReached()
        {
            // Arrange — use a client with SearchMaxSubstringScanItems = 3
            var limitedOptions = new NetAppOptions { Url = TestUrl, RegionName = RegionName, SearchMaxSubstringScanItems = 3 };
            var limitedOptionsMock = new Mock<IOptions<NetAppOptions>>();
            limitedOptionsMock.Setup(x => x.Value).Returns(limitedOptions);
            var limitedClient = new NetAppClient(
                _loggerMock.Object, limitedOptionsMock.Object, _amazonS3UtilsWrapperMock.Object,
                _netAppRequestFactoryMock.Object, _netAppArgFactoryMock.Object, _s3ClientFactoryMock.Object,
                _netAppS3HttpClientMock.Object, _netAppS3HttpArgFactoryMock.Object);

            var arg = CreateSearchArg(SearchModes.Substring, query: "file", maxResults: 3);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page1" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "page1" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 3, "test-operation", false))
                .Returns(listArg1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/file1.txt", "test-operation/file2.txt", "test-operation/file3.txt"],
                    nextContinuationToken: "token-page2",  // more pages exist
                    keyCount: 3));

            // Act
            var result = await limitedClient.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.Truncated);
            Assert.Equal(3, result.TotalScanned);
            // Only one page was fetched despite more pages being available
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 3, "test-operation", false), Times.Once);
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_ReturnsEmptyResult_WhenFirstPageIsNull()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Substring, query: "file");
            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 100, "test-operation", false))
                .Returns(listArg);
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request());
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ThrowsAsync(new AmazonS3Exception("error")); // causes ListObjectsInBucketAsync to return null

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.Data);
            Assert.False(result.Truncated);
            Assert.Equal(0, result.TotalScanned);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_AccumulatesResultsAcrossPages()
        {
            // Arrange
            var arg = CreateSearchArg(SearchModes.Substring, query: "match", maxResults: 5);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page1" };
            var listArg2 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page2" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "page1" };
            var s3Request2 = new ListObjectsV2Request { BucketName = "page2" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 5, "test-operation", false))
                .Returns(listArg1);
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", 5, "test-operation", false))
                .Returns(listArg2);

            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg2)).Returns(s3Request2);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match1.txt", "test-operation/will-not-find.txt"],
                    nextContinuationToken: "token-page2",
                    keyCount: 2));
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request2, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match2.txt", "test-operation/match3.txt"],
                    keyCount: 2));

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result!.Data.Count());
            Assert.All(result.Data, x => Assert.Contains("match", x.Key));
            Assert.Equal(4, result.TotalScanned);
            Assert.False(result.Truncated);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_PageSizeCapedByRemainingLimit()
        {
            // Arrange — SearchMaxSubstringScanItems = 5, maxResults = 10, so second page pageSize = 5 - 3 = 2
            var limitedOptions = new NetAppOptions { Url = TestUrl, RegionName = RegionName, SearchMaxSubstringScanItems = 5 };
            var limitedOptionsMock = new Mock<IOptions<NetAppOptions>>();
            limitedOptionsMock.Setup(x => x.Value).Returns(limitedOptions);
            var limitedClient = new NetAppClient(
                _loggerMock.Object, limitedOptionsMock.Object, _amazonS3UtilsWrapperMock.Object,
                _netAppRequestFactoryMock.Object, _netAppArgFactoryMock.Object, _s3ClientFactoryMock.Object,
                _netAppS3HttpClientMock.Object, _netAppS3HttpArgFactoryMock.Object);

            var arg = CreateSearchArg(SearchModes.Substring, query: "file", maxResults: 10);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page1" };
            var listArg2 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page2" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "page1" };
            var s3Request2 = new ListObjectsV2Request { BucketName = "page2" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 5, "test-operation", false))
                .Returns(listArg1);
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", 2, "test-operation", false))
                .Returns(listArg2);

            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg2)).Returns(s3Request2);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/file1.txt", "test-operation/file2.txt", "test-operation/file3.txt"],
                    nextContinuationToken: "token-page2",
                    keyCount: 3));
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request2, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/file4.txt", "test-operation/file5.txt"],
                    keyCount: 2));

            // Act
            var result = await limitedClient.SearchObjectsInBucketAsync(arg);

            // Assert — second page was requested with pageSize 2, not 10
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", 2, "test-operation", false),
                Times.Once);
            Assert.Equal(5, result!.TotalScanned);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_StopsAndSetsTruncated_WhenMaxResultsCapReached()
        {
            // Arrange — MaxResults=2; page 1 returns exactly 2 matches but a further page exists
            var arg = CreateSearchArg(SearchModes.Substring, query: "match", maxResults: 2);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "page1" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "page1" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false))
                .Returns(listArg1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match1.txt", "test-operation/match2.txt"],
                    nextContinuationToken: "token-page2",
                    keyCount: 2));

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert — should stop after page 1 and mark as truncated
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.True(result.Truncated);
            Assert.Equal(2, result.TotalScanned);
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-page2", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_TrimsResultsAndSetsTruncated_WhenSinglePageExceedsMaxResults()
        {
            // Arrange — MaxResults=2 but page returns 3 matches
            var arg = CreateSearchArg(SearchModes.Substring, query: "match", maxResults: 2);

            var listArg = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = BucketName };
            var s3Request = new ListObjectsV2Request();
            var s3Response = CreateListObjectsV2Response(
                fileKeys: ["test-operation/match1.txt", "test-operation/match2.txt", "test-operation/match3.txt"],
                keyCount: 3);

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false))
                .Returns(listArg);
            SetupListObjectsV2(s3Request, s3Response);

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert — only 2 results returned despite 3 matching; truncated because more existed
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.True(result.Truncated);
            Assert.Equal(3, result.TotalScanned);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_StopsAtMaxResults()
        {
            // Arrange — MaxResults=2; matches arrive one per page so the cap is hit at a page
            // boundary (after page 2). Verifies that page 3 is never requested.
            var arg = CreateSearchArg(SearchModes.Substring, query: "match", maxResults: 2);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "p1" };
            var listArg2 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "p2" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "p1" };
            var s3Request2 = new ListObjectsV2Request { BucketName = "p2" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false))
                .Returns(listArg1);
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-2", 2, "test-operation", false))
                .Returns(listArg2);

            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg2)).Returns(s3Request2);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match1.txt", "test-operation/no-match.txt"],
                    nextContinuationToken: "token-2",
                    keyCount: 2));
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request2, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match2.txt", "test-operation/no-match-2.txt"],
                    nextContinuationToken: "token-3",   // page 3 exists but must never be fetched
                    keyCount: 2));

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert — exactly 2 matching results; page 3 never requested
            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count());
            Assert.All(result.Data, x => Assert.Contains("match", x.Key));
            _netAppArgFactoryMock.Verify(
                f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-3", It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_SubstringMode_SetsTruncatedTrue_WhenCappedByMaxResults()
        {
            var arg = CreateSearchArg(SearchModes.Substring, query: "match", maxResults: 2);

            var listArg1 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "p1" };
            var listArg2 = new ListObjectsInBucketArg { BearerToken = BearerToken, BucketName = "p2" };
            var s3Request1 = new ListObjectsV2Request { BucketName = "p1" };
            var s3Request2 = new ListObjectsV2Request { BucketName = "p2" };

            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, null, 2, "test-operation", false))
                .Returns(listArg1);
            _netAppArgFactoryMock
                .Setup(f => f.CreateListObjectsInBucketArg(BearerToken, BucketName, "token-2", 2, "test-operation", false))
                .Returns(listArg2);

            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg1)).Returns(s3Request1);
            _netAppRequestFactoryMock.Setup(f => f.ListObjectsInBucketRequest(listArg2)).Returns(s3Request2);

            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request1, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match1.txt", "test-operation/random.txt"],
                    nextContinuationToken: "token-2",
                    keyCount: 2));
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(s3Request2, default))
                .ReturnsAsync(CreateListObjectsV2Response(
                    fileKeys: ["test-operation/match2.txt"],
                    nextContinuationToken: "token-3",   // more pages exist beyond the cap
                    keyCount: 1));

            // Act
            var result = await _client.SearchObjectsInBucketAsync(arg);

            // Assert — Truncated=true because MaxResults was reached, not the scan limit
            Assert.NotNull(result);
            Assert.True(result!.Truncated);
            Assert.Equal(2, result.Data.Count());
            // TotalScanned (3) is far below the default SearchMaxSubstringScanItems (10000),
            // confirming MaxResults is the reason for truncation.
            Assert.Equal(3, result.TotalScanned);
            Assert.True(result.TotalScanned < 10000, "Truncated must be caused by MaxResults, not scan limit.");
        }

        [Fact]
        public async Task SearchObjectsInBucketAsync_ThrowsAndLogs_WhenAmazonS3ExceptionEscapes()
        {
            var arg = CreateSearchArg(SearchModes.Prefix, query: "doc");

            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(new ListObjectsV2Request());

            var callCount = 0;
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var errorCode = callCount == 1 ? S3ErrorCodes.InvalidAccessKeyId : S3ErrorCodes.AccessDenied;
                    throw new AmazonS3Exception("S3 error")
                    { StatusCode = HttpStatusCode.Forbidden, ErrorCode = errorCode };
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.SearchObjectsInBucketAsync(arg));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<NetAppAccessDeniedException>(ex);
            _amazonS3Mock.Verify(s => s.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Exactly(2));
            _loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to search objects in bucket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AbortMultipartUploadAsync_ReturnsSuccessfully_OnHappyPath()
        {
            // Arrange
            var arg = new AbortMultipartUploadArg
            { BearerToken = BearerToken, BucketName = BucketName, ObjectKey = "file.txt", UploadId = "upload-id" };

            _amazonS3Mock
                .Setup(s => s.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AbortMultipartUploadResponse());

            // Act
            var ex = await Record.ExceptionAsync(() => _client.AbortMultipartUploadAsync(arg));

            // Assert
            Assert.Null(ex);
            _amazonS3Mock.Verify(
                s => s.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AbortMultipartUploadAsync_DoesNotThrow_OnAmazonS3Exception()
        {
            // Arrange
            var arg = new AbortMultipartUploadArg
            { BearerToken = BearerToken, BucketName = BucketName, ObjectKey = "file.txt", UploadId = "upload-id" };

            _amazonS3Mock
                .Setup(s => s.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("NoSuchUpload")
                { StatusCode = HttpStatusCode.NotFound, ErrorCode = "NoSuchUpload" });

            // Act — abort is best-effort; the method must not propagate the exception
            var ex = await Record.ExceptionAsync(() => _client.AbortMultipartUploadAsync(arg));

            // Assert
            Assert.Null(ex);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v!.ToString()!.Contains("Failed to abort multipart upload")),
                    It.IsAny<AmazonS3Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AbortMultipartUploadAsync_WhenCredentialError_InvalidatesAndRetries()
        {
            // Arrange
            var arg = new AbortMultipartUploadArg
            { BearerToken = BearerToken, BucketName = BucketName, ObjectKey = "file.txt", UploadId = "upload-id" };

            var callCount = 0;
            _amazonS3Mock
                .Setup(s => s.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new AmazonS3Exception("The AWS access key ID you provided does not exist in our records.")
                        { StatusCode = HttpStatusCode.Forbidden, ErrorCode = S3ErrorCodes.InvalidAccessKeyId };
                    return new AbortMultipartUploadResponse();
                });

            // Act
            var ex = await Record.ExceptionAsync(() => _client.AbortMultipartUploadAsync(arg));

            // Assert
            Assert.Null(ex);
            _s3ClientFactoryMock.Verify(x => x.InvalidateClientAsync(), Times.Once);
            _amazonS3Mock.Verify(
                s => s.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        private static SearchArg CreateSearchArg(SearchModes mode, string? query = null, int maxResults = 100)
            => new()
            {
                BearerToken = BearerToken,
                BucketName = BucketName,
                OperationName = "test-operation",
                Query = query,
                MaxResults = maxResults,
                Mode = mode
            };

        private static ListObjectsV2Response CreateListObjectsV2Response(
            IList<string>? fileKeys = null,
            IList<string>? folderPrefixes = null,
            string? nextContinuationToken = null,
            int? keyCount = null)
        {
            var files = (fileKeys ?? []).Select(k => new S3Object { Key = k, Size = 100, LastModified = DateTime.UtcNow }).ToList();
            var prefixes = (folderPrefixes ?? []).ToList();
            return new ListObjectsV2Response
            {
                S3Objects = files,
                CommonPrefixes = prefixes,
                NextContinuationToken = nextContinuationToken,
                KeyCount = keyCount ?? files.Count + prefixes.Count
            };
        }

        private void SetupListObjectsV2(ListObjectsV2Request request, ListObjectsV2Response response)
        {
            _netAppRequestFactoryMock
                .Setup(f => f.ListObjectsInBucketRequest(It.IsAny<ListObjectsInBucketArg>()))
                .Returns(request);
            _amazonS3Mock
                .Setup(s => s.ListObjectsV2Async(request, default))
                .ReturnsAsync(response);
        }

        private void SetupFileExistsCheck(string filePath, bool exists)
        {
            var getObjectArg = new GetObjectArg { BearerToken = BearerToken, BucketName = BucketName, ObjectKey = filePath };
            var headObjectArg = new GetHeadObjectArg { BearerToken = BearerToken, BucketName = BucketName, ObjectKey = filePath };

            _netAppArgFactoryMock
                .Setup(x => x.CreateGetObjectArg(BearerToken, BucketName, filePath, null))
                .Returns(getObjectArg);

            _netAppS3HttpArgFactoryMock
                .Setup(x => x.CreateGetHeadObjectArg(BearerToken, BucketName, filePath))
                .Returns(headObjectArg);

            _netAppS3HttpClientMock
                .Setup(x => x.GetHeadObjectAsync(headObjectArg))
                .ReturnsAsync(new HeadObjectResponseDto
                {
                    StatusCode = exists ? HttpStatusCode.OK : HttpStatusCode.NotFound
                });
        }
    }
}
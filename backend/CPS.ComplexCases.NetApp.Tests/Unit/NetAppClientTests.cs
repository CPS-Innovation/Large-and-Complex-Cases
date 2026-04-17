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

            _client = new NetAppClient(_loggerMock.Object, _amazonS3UtilsWrapperMock.Object,
                _netAppRequestFactoryMock.Object, _s3ClientFactoryMock.Object, _netAppS3HttpClientMock.Object,
                _netAppS3HttpArgFactoryMock.Object);
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
            // Arrange
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

            // Assert
            Assert.True(result.Success);
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
            // Arrange
            var folderPath = "witnesses/empty";
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

            // Assert
            Assert.True(result.Success);
            _amazonS3Mock.Verify(x => x.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
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
    }
}
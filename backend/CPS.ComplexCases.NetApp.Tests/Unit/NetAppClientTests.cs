using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit
{
    public class NetAppClientTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ILogger<NetAppClient>> _loggerMock;
        private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
        private readonly Mock<IAmazonS3UtilsWrapper> _amazonS3UtilsWrapperMock;
        private readonly Mock<IAmazonS3> _amazonS3Mock;
        private readonly Mock<INetAppRequestFactory> _netAppRequestFactoryMock;
        private readonly NetAppClient _client;
        private const string _testUrl = "https://netapp.com";
        private const string _accessKey = "accessKey";
        private const string _secretKey = "secretKey";
        private const string _regionName = "eu-west-2";
        private const string _bucketName = "test-bucket";

        public NetAppClientTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _loggerMock = _fixture.Freeze<Mock<ILogger<NetAppClient>>>();
            var options = new NetAppOptions
            {
                Url = _testUrl,
                AccessKey = _accessKey,
                SecretKey = _secretKey,
                RegionName = _regionName,
                BucketName = _bucketName
            };
            _optionsMock = new Mock<IOptions<NetAppOptions>>();
            _optionsMock.Setup(x => x.Value).Returns(options);
            _amazonS3UtilsWrapperMock = _fixture.Freeze<Mock<IAmazonS3UtilsWrapper>>();
            _amazonS3Mock = _fixture.Freeze<Mock<IAmazonS3>>();
            _netAppRequestFactoryMock = _fixture.Freeze<Mock<INetAppRequestFactory>>();

            _client = new NetAppClient(_loggerMock.Object, _amazonS3Mock.Object, _amazonS3UtilsWrapperMock.Object, _netAppRequestFactoryMock.Object);
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
            _amazonS3UtilsWrapperMock.Setup(x => x.DoesS3BucketExistV2Async(It.IsAny<AmazonS3Client>(), arg.BucketName))
                .ReturnsAsync(false);

            _amazonS3Mock.Setup(x => x.PutBucketAsync(It.IsAny<PutBucketRequest>(), default))
                .ReturnsAsync(new PutBucketResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
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
        public async Task GetObjectAsync_WhenObjectDoesNotExist_ReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<GetObjectArg>();
            var expectedExceptionMessage = $"Failed to get file {arg.ObjectKey} from bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.GetObjectAsync(arg);

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
        public async Task GetObjectAsync_WhenExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<GetObjectArg>();
            var expectedExceptionMessage = "Error";

            _amazonS3Mock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.GetObjectAsync(arg);

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
        public async Task UploadObjectAsync_WhenUploadSucceeds_ReturnsTrue()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            var putObjectResponse = new PutObjectResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
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
            var putObjectResponse = new PutObjectResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest
            };

            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(putObjectResponse);

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UploadObjectAsync_WhenExceptionThrown_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var arg = _fixture.Create<UploadObjectArg>();
            var expectedExceptionMessage = $"Failed to upload file {arg.ObjectKey} to bucket {arg.BucketName}.";

            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception(expectedExceptionMessage));

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            Assert.False(result);
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
            Assert.Empty(result?.Data.FileData);
        }

        [Fact]
        public async Task ListObjectsInBucketAsync_WhenExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var arg = _fixture.Create<ListObjectsInBucketArg>();
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
            var listObjectsResponse = new ListObjectsV2Response
            {
                CommonPrefixes = new List<string> { "folder1/", "folder2/" }
            };

            _amazonS3Mock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(listObjectsResponse);

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);
            var data = result?.Data.FolderData?.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, data?.Count());
            Assert.Contains("folder1/", data[0].Path);
            Assert.Contains("folder2/", data[1].Path);
        }

        [Fact]
        public async Task ListFoldersInBucketAsync_WhenNoFoldersExist_ReturnsEmptyList()
        {
            // Arrange
            var arg = _fixture.Create<ListFoldersInBucketArg>();
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
    }
}
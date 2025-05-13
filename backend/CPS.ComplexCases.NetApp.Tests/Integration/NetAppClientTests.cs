using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.WireMock.Mappings;
using CPS.ComplexCases.NetApp.Wrappers;
using CPS.ComplexCases.WireMock.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.Tests.Integration
{
    public class NetAppClientTests
    {
        private readonly WireMockServer _server;
        private readonly NetAppClient _client;
        private readonly NetAppArgFactory _netAppArgFactory;
        private readonly INetAppRequestFactory _netAppRequestFactory;
        private readonly AmazonS3Client _s3Client;
        private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper;

        public NetAppClientTests()
        {
            _server = WireMockServer
                .Start()
                .LoadMappings(
                    new BucketMapping(),
                    new ObjectMapping()
                );

            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = _server.Urls[0],
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials("fakeAccessKey", "fakeSecretKey");
            _s3Client = new AmazonS3Client(credentials, s3ClientConfig);
            _amazonS3UtilsWrapper = new AmazonS3UtilsWrapper();
            _netAppArgFactory = new NetAppArgFactory();
            _netAppRequestFactory = new NetAppRequestFactory();

            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

            _client = new NetAppClient(logger, _s3Client, _amazonS3UtilsWrapper, _netAppRequestFactory);
            _netAppArgFactory = new NetAppArgFactory();
        }

        [Fact]
        public async Task CreateBucket_WhenBucketDoesNotAlreadyExist_ReturnsTrue()
        {
            // Arrange
            var bucketName = "test-bucket";
            var arg = _netAppArgFactory.CreateCreateBucketArg(bucketName);

            // Act
            var result = await _client.CreateBucketAsync(arg);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task FindBucket_WhenBucketExists_ReturnsBucket()
        {
            // Arrange
            var bucketName = "test-bucket";
            var arg = _netAppArgFactory.CreateFindBucketArg(bucketName);

            // Act
            var result = await _client.FindBucketAsync(arg);

            // Assert
            result.Should().NotBeNull();
            result.BucketName.Should().Be(bucketName);
        }

        [Fact]
        public async Task ListBuckets_WhenBucketsExist_ReturnsBuckets()
        {
            // Act
            var result = await _client.ListBucketsAsync(_netAppArgFactory.CreateListBucketsArg());

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task UploadObject_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-file.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test upload!"));
            var arg = _netAppArgFactory.CreateUploadObjectArg(bucketName, objectName, stream);

            // Act
            var result = await _client.UploadObjectAsync(arg);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ListObjects_WhenObjectsExists_ReturnsObjects()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-file.txt";
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(bucketName);

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            result.Should().NotBeNull();
            result.BucketName.Should().Be(bucketName);
            result.FileData.Should().HaveCount(2);
            result.FileData.ToList()[0].Key.Should().Be(objectName);
        }

        [Fact]
        public async Task GetObject_WhenObjectExists_ReturnsObject()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-document.pdf";
            var arg = _netAppArgFactory.CreateGetObjectArg(bucketName, objectName);

            // Act
            var result = await _client.GetObjectAsync(arg);

            // Assert
            result.Should().NotBeNull();
            result.BucketName.Should().Be(bucketName);
            result.Key.Should().Be(objectName);
        }

        [Fact]
        public async Task ListNestedObjects_WhenObjectsExists_ReturnsObjects()
        {
            // Arrange
            var bucketName = "nested-objects";
            var arg = _netAppArgFactory.CreateListFoldersInBucketArg(bucketName);

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);

            // Assert
            result.Should().NotBeNull();
            result.FolderData.Should().HaveCount(3);
            result.FolderData.Should().Contain(x => x.Path == "counsel/");
            result.FolderData.Should().Contain(x => x.Path == "counsel/statements/");
            result.FolderData.Should().Contain(x => x.Path == "multimedia/");
        }
    }
}
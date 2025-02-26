using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.NetApp.Integration
{
    public class NetAppClientTests
    {
        private readonly NetAppClient _client;
        private readonly NetAppArgFactory _netAppArgFactory;
        private readonly AmazonS3Client _s3Client;
        private readonly IAmazonS3UtilsWrapper _amazonS3UtilsWrapper;

        public NetAppClientTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var netAppOptions = configuration.GetSection("NetAppOptions").Get<NetAppOptions>() ?? throw new InvalidOperationException("NetAppOptions section is missing or invalid.");

            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = netAppOptions.Url,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials("fakeAccessKey", "fakeSecretKey");
            _s3Client = new AmazonS3Client(credentials, s3ClientConfig);
            _amazonS3UtilsWrapper = new AmazonS3UtilsWrapper();

            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

            _client = new NetAppClient(logger, _s3Client, _amazonS3UtilsWrapper);
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
            Assert.True(result);
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
            Assert.NotNull(result);
            Assert.Equal(bucketName, result!.BucketName);
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
            Assert.True(result);
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
            Assert.NotNull(result);
            Assert.Equal(bucketName, result.Name);
            Assert.Equal(2, result.S3Objects.Count);
            Assert.Equal(objectName, result.S3Objects[0].Key);
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
            Assert.NotNull(result);
            Assert.Equal(bucketName, result.BucketName);
            Assert.Equal(objectName, result.Key);
        }
    }
}
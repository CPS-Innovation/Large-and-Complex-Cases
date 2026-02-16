using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using CPS.ComplexCases.NetApp.Services;
using CPS.ComplexCases.NetApp.Telemetry;
using CPS.ComplexCases.NetApp.WireMock.Mappings;
using CPS.ComplexCases.NetApp.Wrappers;
using CPS.ComplexCases.WireMock.Core;
using Moq;
using WireMock.Server;
using System.Security.Cryptography.X509Certificates;

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
        private readonly IS3ClientFactory _s3ClientFactory;
        private readonly Mock<IS3CredentialService> _mockCredentialService;
        private readonly Mock<IKeyVaultService> _mockKeyVaultService;
        private readonly Mock<IS3TelemetryHandler> _mockTelemetryHandler;
        private readonly INetAppS3HttpClient _netAppS3HttpClient;
        private readonly INetAppS3HttpArgFactory _netAppS3HttpArgFactory;
        private readonly Mock<INetAppCertFactory> _mockNetAppCertFactory;
        private const string TestOid = "test-oid-12345";
        private const string TestUserName = "testuser@example.com";
        private static readonly string BearerToken = GenerateTestJwtToken(TestOid, TestUserName);

        private static string GenerateTestJwtToken(string oid, string preferredUsername)
        {
            var claims = new[]
            {
                new Claim("oid", oid),
                new Claim("preferred_username", preferredUsername)
            };

            var key = new SymmetricSecurityKey("test-signing-key-that-is-at-least-32-bytes-long"u8.ToArray());
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "test-issuer",
                audience: "test-audience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

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
            var s3ClientFactoryLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<S3ClientFactory>();
            _netAppS3HttpClient = new NetAppS3HttpClient(
                new HttpClient { BaseAddress = new Uri(_server.Urls[0]) },
                null!,
                Options.Create(new NetAppOptions
                {
                    Url = _server.Urls[0],
                    RegionName = "eu-west-1"
                })
            );
            _netAppS3HttpArgFactory = new NetAppS3HttpArgFactory();

            // Setup mock credential service
            _mockCredentialService = new Mock<IS3CredentialService>();
            _mockKeyVaultService = new Mock<IKeyVaultService>();
            _mockTelemetryHandler = new Mock<IS3TelemetryHandler>();
            _mockNetAppCertFactory = new Mock<INetAppCertFactory>();

            var fakeCredentials = new S3CredentialsDecrypted
            {
                AccessKey = "fakeAccessKey",
                SecretKey = "fakeSecretKey",
                Metadata = new S3CredentialsMetadata
                {
                    UserPrincipalName = TestUserName,
                    Salt = "fakeSalt",
                    CreatedAt = DateTime.UtcNow,
                    LastRotated = null,
                    PepperVersion = "v1"
                }
            };

            _mockCredentialService
                .Setup(x => x.GetCredentialsAsync(
                    TestOid,
                    TestUserName,
                    It.IsAny<string>()))
                .ReturnsAsync(fakeCredentials);

            // Setup credential status to return valid credentials (not needing regeneration)
            _mockKeyVaultService
                .Setup(x => x.CheckCredentialStatusAsync(It.IsAny<string>()))
                .ReturnsAsync(new CredentialStatus
                {
                    Exists = true,
                    IsValid = true,
                    NeedsRegeneration = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(55),
                    RemainingMinutes = 55
                });

            _s3ClientFactory = new S3ClientFactory(
                Options.Create(new NetAppOptions
                {
                    Url = _server.Urls[0],
                    RegionName = "eu-west-1"
                }),
                _mockCredentialService.Object,
                _mockKeyVaultService.Object,
                s3ClientFactoryLogger,
                _mockTelemetryHandler.Object,
                _mockNetAppCertFactory.Object
            );
            _s3ClientFactory.SetS3ClientAsync(_s3Client);

            _netAppS3HttpClient = new NetAppS3HttpClient(
                new HttpClient { BaseAddress = new Uri(_server.Urls[0]) },
                _mockCredentialService.Object,
                Options.Create(new NetAppOptions
                {
                    Url = _server.Urls[0],
                    RegionName = "eu-west-1"
                })
            );

            _netAppS3HttpArgFactory = new NetAppS3HttpArgFactory();

            var testCert = new X509Certificate2([]);
            var testCertCollection = new X509Certificate2Collection { testCert };

            _mockNetAppCertFactory
                .Setup(x => x.GetTrustedCaCertificates())
                .Returns(testCertCollection);


            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NetAppClient>();

            _client = new NetAppClient(logger, _amazonS3UtilsWrapper, _netAppRequestFactory, _s3ClientFactory, _netAppS3HttpClient, _netAppS3HttpArgFactory);
        }

        [Fact]
        public async Task CreateBucket_WhenBucketDoesNotAlreadyExist_ReturnsTrue()
        {
            // Arrange
            var bucketName = "test-bucket";
            var arg = _netAppArgFactory.CreateCreateBucketArg(BearerToken, bucketName);

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
            var arg = _netAppArgFactory.CreateFindBucketArg(BearerToken, bucketName);

            // Act
            var result = await _client.FindBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bucketName, result.BucketName);
        }

        [Fact]
        public async Task ListBuckets_WhenBucketsExist_ReturnsBuckets()
        {
            // Act
            var result = await _client.ListBucketsAsync(_netAppArgFactory.CreateListBucketsArg(BearerToken));

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        [Fact]
        public async Task UploadObject_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-file.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test upload!"));
            var contentLength = stream.Length;
            var arg = _netAppArgFactory.CreateUploadObjectArg(BearerToken, bucketName, objectName, stream, contentLength, false);

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
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(BearerToken, bucketName);

            // Act
            var result = await _client.ListObjectsInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bucketName, result.Data.BucketName);
            Assert.Equal(2, result.Data.FileData.Count());
            Assert.Equal(objectName, result.Data.FileData.ToList()[0].Path);
        }

        [Fact]
        public async Task GetObject_WhenObjectExists_ReturnsObject()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-document.pdf";
            var arg = _netAppArgFactory.CreateGetObjectArg(BearerToken, bucketName, objectName);

            // Act
            var result = await _client.GetObjectAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bucketName, result.BucketName);
            Assert.Equal(objectName, result.Key);
        }

        [Fact]
        public async Task ListNestedObjects_WhenObjectsExists_ReturnsObjects()
        {
            // Arrange
            var bucketName = "nested-objects";
            var arg = _netAppArgFactory.CreateListFoldersInBucketArg(BearerToken, bucketName);

            // Act
            var result = await _client.ListFoldersInBucketAsync(arg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.FolderData.Count());
            Assert.Contains(result.Data.FolderData, x => x.Path == "counsel/");
            Assert.Contains(result.Data.FolderData, x => x.Path == "counsel/statements/");
            Assert.Contains(result.Data.FolderData, x => x.Path == "multimedia/");
        }
    }
}
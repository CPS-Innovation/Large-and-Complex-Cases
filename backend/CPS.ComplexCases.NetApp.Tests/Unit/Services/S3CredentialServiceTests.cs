using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using CPS.ComplexCases.NetApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Services;

public class S3CredentialServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IKeyVaultService> _keyVaultServiceMock;
    private readonly Mock<INetAppHttpClient> _netAppHttpClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ICryptographyService> _cryptographyServiceMock;
    private readonly Mock<ILogger<S3CredentialService>> _loggerMock;
    private readonly NetAppOptions _options;
    private readonly S3CredentialService _sut;

    private readonly string _oid;
    private readonly string _userName;
    private readonly string _bearerToken;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _pepper;
    private readonly byte[] _saltBytes;
    private readonly string _saltBase64;
    private readonly string _encryptedAccessKey;
    private readonly string _encryptedSecretKey;

    public S3CredentialServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _keyVaultServiceMock = new Mock<IKeyVaultService>();
        _netAppHttpClientMock = new Mock<INetAppHttpClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _cryptographyServiceMock = new Mock<ICryptographyService>();
        _loggerMock = new Mock<ILogger<S3CredentialService>>();

        _options = new NetAppOptions
        {
            S3ServiceUuid = _fixture.Create<Guid>(),
            Url = "https://netapp.example.com",
            RegionName = "eu-west-1",
            PepperVersion = "v1",
            SessionDurationSeconds = 3600
        };

        var optionsMock = new Mock<IOptions<NetAppOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _sut = new S3CredentialService(
            _keyVaultServiceMock.Object,
            _netAppHttpClientMock.Object,
            _netAppArgFactoryMock.Object,
            _cryptographyServiceMock.Object,
            optionsMock.Object,
            _loggerMock.Object);

        _oid = _fixture.Create<string>();
        _userName = "test.user@example.com";
        _bearerToken = _fixture.Create<string>();
        _accessKey = _fixture.Create<string>();
        _secretKey = _fixture.Create<string>();
        _pepper = _fixture.Create<string>();
        _saltBytes = _fixture.CreateMany<byte>(16).ToArray();
        _saltBase64 = Convert.ToBase64String(_saltBytes);
        _encryptedAccessKey = _fixture.Create<string>();
        _encryptedSecretKey = _fixture.Create<string>();
    }

    [Fact]
    public async Task GetCredentialsAsync_WithValidExistingCredentials_ReturnsDecryptedCredentials()
    {
        // Arrange
        var status = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            RemainingMinutes = 30
        };

        var encryptedCreds = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _encryptedAccessKey,
            EncryptedSecretKey = _encryptedSecretKey,
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = _userName,
                Salt = _saltBase64,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                PepperVersion = "v1"
            }
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetCredentialsAsync(_oid))
            .ReturnsAsync(encryptedCreds);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _cryptographyServiceMock.Setup(x => x.DecryptAsync(_encryptedAccessKey, _oid, _saltBase64, _pepper))
            .ReturnsAsync(_accessKey);
        _cryptographyServiceMock.Setup(x => x.DecryptAsync(_encryptedSecretKey, _oid, _saltBase64, _pepper))
            .ReturnsAsync(_secretKey);

        // Act
        var result = await _sut.GetCredentialsAsync(_oid, _userName, _bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_accessKey, result.AccessKey);
        Assert.Equal(_secretKey, result.SecretKey);
        Assert.Equal(_userName, result.Metadata.UserPrincipalName);

        _netAppHttpClientMock.Verify(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()), Times.Never);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithNonExistingCredentials_RegeneratesAndStoresNew()
    {
        // Arrange
        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = _accessKey, SecretKey = _secretKey, Name = _userName }
            }
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .Returns(Task.CompletedTask);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(_accessKey, _oid, _saltBytes, _pepper))
            .ReturnsAsync(_encryptedAccessKey);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(_secretKey, _oid, _saltBytes, _pepper))
            .ReturnsAsync(_encryptedSecretKey);

        // Act
        var result = await _sut.GetCredentialsAsync(_oid, _userName, _bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_accessKey, result.AccessKey);
        Assert.Equal(_secretKey, result.SecretKey);
        Assert.Equal(_userName, result.Metadata.UserPrincipalName);

        _netAppHttpClientMock.Verify(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()), Times.Once);
        _keyVaultServiceMock.Verify(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()), Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithExpiringCredentials_RotatesCredentials()
    {
        // Arrange
        var oldAccessKey = "old-access-key";
        var newAccessKey = "new-access-key";
        var newSecretKey = "new-secret-key";
        var createdAt = DateTime.UtcNow.AddMinutes(-56);

        var status = new CredentialStatus
        {
            Exists = true,
            IsValid = false,
            NeedsRegeneration = true,
            RemainingMinutes = 4
        };

        var existingEncryptedCreds = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = "old-encrypted-access",
            EncryptedSecretKey = "old-encrypted-secret",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = _userName,
                Salt = _saltBase64,
                CreatedAt = createdAt,
                PepperVersion = "v1"
            }
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = newAccessKey, SecretKey = newSecretKey, Name = _userName }
            }
        };

        // Setup CheckCredentialStatusAsync to be called twice due to double-check in lock
        _keyVaultServiceMock.SetupSequence(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status)  // First call in GetCredentialsAsync
            .ReturnsAsync(status); // Second call in RegenerateCredentialsWithLockAsync (double-check)

        _keyVaultServiceMock.Setup(x => x.GetCredentialsAsync(_oid))
            .ReturnsAsync(existingEncryptedCreds);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .Returns(Task.CompletedTask);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.DecryptAsync("old-encrypted-access", _oid, _saltBase64, _pepper))
            .ReturnsAsync(oldAccessKey);
        _cryptographyServiceMock.Setup(x => x.DecryptAsync("old-encrypted-secret", _oid, _saltBase64, _pepper))
            .ReturnsAsync("old-secret-key");
        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(newAccessKey, _oid, _saltBytes, _pepper))
            .ReturnsAsync("new-encrypted-access");
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(newSecretKey, _oid, _saltBytes, _pepper))
            .ReturnsAsync("new-encrypted-secret");

        // Act
        var result = await _sut.GetCredentialsAsync(_oid, _userName, _bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newAccessKey, result.AccessKey);
        Assert.Equal(newSecretKey, result.SecretKey);
        Assert.NotEqual(createdAt, result.Metadata.CreatedAt);
        Assert.NotNull(result.Metadata.LastRotated);

        _netAppHttpClientMock.Verify(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()), Times.Once);
        _keyVaultServiceMock.Verify(x => x.StoreCredentialsAsync(_oid,
            It.Is<S3CredentialsEncrypted>(c =>
                c.Metadata.LastRotated != null)), Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenStatusReportsExistingButRetrievalFails_Regenerates()
    {
        // Arrange
        var validStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            RemainingMinutes = 30
        };

        var invalidStatus = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = _accessKey, SecretKey = _secretKey, Name = _userName}
            }
        };

        // Setup CheckCredentialStatusAsync to be called twice due to double-check in lock
        _keyVaultServiceMock.SetupSequence(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(validStatus)   // First call - reports as existing
            .ReturnsAsync(invalidStatus); // Second call in lock - reports as non-existing after null retrieval

        _keyVaultServiceMock.Setup(x => x.GetCredentialsAsync(_oid))
            .ReturnsAsync((S3CredentialsEncrypted?)null);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .Returns(Task.CompletedTask);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(It.IsAny<string>(), _oid, _saltBytes, _pepper))
            .ReturnsAsync("encrypted-value");

        // Act
        var result = await _sut.GetCredentialsAsync(_oid, _userName, _bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_accessKey, result.AccessKey);

        _netAppHttpClientMock.Verify(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()), Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenNetAppUserNotFound_RegistersNewUser()
    {
        // Arrange
        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = _accessKey, SecretKey = _secretKey, Name = _userName}
            }
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .Returns(Task.CompletedTask);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ThrowsAsync(new NetAppNotFoundException("User not found"));
        _netAppHttpClientMock.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterUserArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());
        _netAppArgFactoryMock.Setup(x => x.CreateRegisterUserArg(_userName, _bearerToken, _options.S3ServiceUuid))
            .Returns(new RegisterUserArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(It.IsAny<string>(), _oid, _saltBytes, _pepper))
            .ReturnsAsync("encrypted-value");

        // Act
        var result = await _sut.GetCredentialsAsync(_oid, _userName, _bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_accessKey, result.AccessKey);

        _netAppHttpClientMock.Verify(x => x.RegisterUserAsync(It.IsAny<RegisterUserArg>()), Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenNetAppClientFails_ThrowsS3CredentialException()
    {
        // Arrange
        var netAppException = new NetAppClientException(
            System.Net.HttpStatusCode.BadRequest,
            new HttpRequestException("NetApp API error"));

        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ThrowsAsync(netAppException);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<S3CredentialException>(() =>
            _sut.GetCredentialsAsync(_oid, _userName, _bearerToken));

        Assert.Contains("Failed to generate S3 credentials from NetApp", ex.Message);
        Assert.Equal(netAppException, ex.InnerException);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenKeyVaultStorageFails_ThrowsS3CredentialException()
    {
        // Arrange
        var keyVaultException = new KeyVaultException("Storage failed");

        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = _accessKey, SecretKey = _secretKey, Name = _userName}
            }
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .ThrowsAsync(keyVaultException);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(It.IsAny<string>(), _oid, _saltBytes, _pepper))
            .ReturnsAsync("encrypted-value");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<S3CredentialException>(() =>
            _sut.GetCredentialsAsync(_oid, _userName, _bearerToken));

        Assert.Contains("Failed to store credentials", ex.Message);
        Assert.Equal(keyVaultException, ex.InnerException);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenDecryptionFails_ThrowsS3CredentialException()
    {
        // Arrange
        var decryptException = new Exception("Decryption failed");

        var status = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            RemainingMinutes = 30
        };

        var encryptedCreds = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _encryptedAccessKey,
            EncryptedSecretKey = _encryptedSecretKey,
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = _userName,
                Salt = _saltBase64,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                PepperVersion = "v1"
            }
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetCredentialsAsync(_oid))
            .ReturnsAsync(encryptedCreds);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _cryptographyServiceMock.Setup(x => x.DecryptAsync(It.IsAny<string>(), _oid, _saltBase64, _pepper))
            .ThrowsAsync(decryptException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<S3CredentialException>(() =>
            _sut.GetCredentialsAsync(_oid, _userName, _bearerToken));

        Assert.Contains("Failed to decrypt S3 credentials", ex.Message);
        Assert.Equal(decryptException, ex.InnerException);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithEmptyNetAppResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>()
        };

        _keyVaultServiceMock.Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(status);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(It.IsAny<string>(), _oid, _saltBytes, _pepper))
            .ReturnsAsync("encrypted-value");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.GetCredentialsAsync(_oid, _userName, _bearerToken));
    }

    [Fact]
    public async Task GetCredentialsAsync_WithConcurrentRequests_OnlyRegeneratesOnce()
    {
        // Arrange
        var status = new CredentialStatus
        {
            Exists = false,
            IsValid = false,
            NeedsRegeneration = true
        };

        var validStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            RemainingMinutes = 30
        };

        var netAppResponse = new NetAppUserResponse
        {
            Records = new List<NetAppUserRecord>
            {
                new NetAppUserRecord { AccessKey = _accessKey, SecretKey = _secretKey, Name = _userName }
            }
        };

        var encryptedCreds = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _encryptedAccessKey,
            EncryptedSecretKey = _encryptedSecretKey,
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = _userName,
                Salt = _saltBase64,
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var callCount = 0;

        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(_oid))
            .ReturnsAsync(() =>
            {
                var count = Interlocked.Increment(ref callCount);

                // First TWO calls must be invalid:
                //  - outside lock
                //  - inside lock
                if (count <= 2)
                {
                    return status; // invalid
                }

                return validStatus; // after regeneration
            });


        _keyVaultServiceMock.Setup(x => x.GetCredentialsAsync(_oid))
            .ReturnsAsync(encryptedCreds);
        _keyVaultServiceMock.Setup(x => x.GetPepperAsync("v1"))
            .ReturnsAsync(_pepper);
        _keyVaultServiceMock.Setup(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()))
            .Returns(Task.CompletedTask);

        _netAppHttpClientMock.Setup(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()))
            .ReturnsAsync(netAppResponse);

        _netAppArgFactoryMock.Setup(x => x.CreateRegenerateUserKeysArg(_userName, _bearerToken, _options.S3ServiceUuid, _options.SessionDurationSeconds))
            .Returns(new RegenerateUserKeysArg());

        _cryptographyServiceMock.Setup(x => x.CreateSalt())
            .Returns(_saltBytes);
        _cryptographyServiceMock.Setup(x => x.EncryptAsync(It.IsAny<string>(), _oid, _saltBytes, _pepper))
            .ReturnsAsync("encrypted-value");
        _cryptographyServiceMock.Setup(x => x.DecryptAsync(It.IsAny<string>(), _oid, _saltBase64, _pepper))
            .ReturnsAsync(_accessKey);

        // Act - Simulate concurrent requests
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _sut.GetCredentialsAsync(_oid, _userName, _bearerToken))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result =>
        {
            Assert.NotNull(result);
            Assert.Equal(_accessKey, result.AccessKey);
        });

        // Verify regeneration happened only once despite concurrent requests
        _netAppHttpClientMock.Verify(x => x.RegenerateUserKeysAsync(It.IsAny<RegenerateUserKeysArg>()), Times.Once);
        _keyVaultServiceMock.Verify(x => x.StoreCredentialsAsync(_oid, It.IsAny<S3CredentialsEncrypted>()), Times.Once);
    }
}
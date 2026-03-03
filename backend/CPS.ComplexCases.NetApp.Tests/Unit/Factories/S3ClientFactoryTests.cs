using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using CPS.ComplexCases.NetApp.Services;
using CPS.ComplexCases.NetApp.Telemetry;
using Moq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Factories;

public class S3ClientFactoryTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
    private readonly Mock<IS3CredentialService> _credentialServiceMock;
    private readonly Mock<IKeyVaultService> _keyVaultServiceMock;
    private readonly Mock<ILogger<S3ClientFactory>> _loggerMock;
    private readonly Mock<IS3TelemetryHandler> _telemetryHandlerMock;
    private readonly Mock<INetAppCertFactory> _netAppCertFactoryMock;
    private readonly S3ClientFactory _factory;
    private const string TestOid = "test-oid-12345";
    private const string TestUserName = "testuser@example.com";

    public S3ClientFactoryTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        var options = new NetAppOptions
        {
            Url = "https://netapp.test.local",
            RegionName = "eu-west-2"
        };
        _optionsMock = new Mock<IOptions<NetAppOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(options);

        _credentialServiceMock = new Mock<IS3CredentialService>();
        _keyVaultServiceMock = new Mock<IKeyVaultService>();
        _loggerMock = new Mock<ILogger<S3ClientFactory>>();
        _telemetryHandlerMock = new Mock<IS3TelemetryHandler>();
        _netAppCertFactoryMock = new Mock<INetAppCertFactory>();

        var testCert = new X509Certificate2([]);
        var testCertCollection = new X509Certificate2Collection { testCert };

        _netAppCertFactoryMock
            .Setup(x => x.GetTrustedCaCertificates())
            .Returns(testCertCollection);

        _factory = new S3ClientFactory(
            _optionsMock.Object,
            _credentialServiceMock.Object,
            _keyVaultServiceMock.Object,
            _loggerMock.Object,
            _telemetryHandlerMock.Object,
            _netAppCertFactoryMock.Object);
    }

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

    [Fact]
    public async Task GetS3ClientAsync_WhenNoClientCached_CreatesNewClient()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken))
            .ReturnsAsync((credentials.AccessKey, credentials.SecretKey));

        // Act
        var client = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.NotNull(client);
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Once);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenCachedClientValid_ReturnsCachedClient()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var validStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            NeedsRegeneration = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(55),
            RemainingMinutes = 55
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken))
            .ReturnsAsync((credentials.AccessKey, credentials.SecretKey));

        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(TestOid))
            .ReturnsAsync(validStatus);

        var firstClient = await _factory.GetS3ClientAsync(bearerToken);

        // Act - Second call should return cached client
        var secondClient = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.Same(firstClient, secondClient);
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Once);
        _keyVaultServiceMock.Verify(
            x => x.CheckCredentialStatusAsync(TestOid),
            Times.Once);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenCredentialsNeedRegeneration_RecreatesClient()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var expiringStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = false,
            NeedsRegeneration = true,
            CreatedAt = DateTime.UtcNow.AddMinutes(-58),
            ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            RemainingMinutes = 2
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken))
            .ReturnsAsync((credentials.AccessKey, credentials.SecretKey));

        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(TestOid))
            .ReturnsAsync(expiringStatus);

        var firstClient = await _factory.GetS3ClientAsync(bearerToken);

        // Act - Second call should recreate client due to expiring credentials
        var secondClient = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.NotSame(firstClient, secondClient);
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Exactly(2)); // Called twice - once for each client creation
        _keyVaultServiceMock.Verify(
            x => x.CheckCredentialStatusAsync(TestOid),
            Times.Once);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenCredentialsWithin5MinutesOfExpiry_RecreatesClient()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var nearExpiryStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = false,
            NeedsRegeneration = true,
            CreatedAt = DateTime.UtcNow.AddMinutes(-56),
            ExpiresAt = DateTime.UtcNow.AddMinutes(4),
            RemainingMinutes = 4
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken))
            .ReturnsAsync((credentials.AccessKey, credentials.SecretKey));

        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(TestOid))
            .ReturnsAsync(nearExpiryStatus);

        await _factory.GetS3ClientAsync(bearerToken);

        // Act - Second call should recreate client
        await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Exactly(2));
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credentials expiring soon")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenDifferentUser_CreatesNewClient()
    {
        // Arrange
        var oid1 = "oid-user-1";
        var oid2 = "oid-user-2";
        var bearerToken1 = GenerateTestJwtToken(oid1, "user1@example.com");
        var bearerToken2 = GenerateTestJwtToken(oid2, "user2@example.com");

        var credentials1 = new S3CredentialsDecrypted
        {
            AccessKey = "access-key-1",
            SecretKey = "secret-key-1",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = "user1@example.com",
                Salt = "salt-1",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var credentials2 = new S3CredentialsDecrypted
        {
            AccessKey = "access-key-2",
            SecretKey = "secret-key-2",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = "user2@example.com",
                Salt = "salt-2",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken1))
            .ReturnsAsync((credentials1.AccessKey, credentials1.SecretKey));

        _credentialServiceMock
            .Setup(x => x.GetCredentialKeysAsync(bearerToken2))
            .ReturnsAsync((credentials2.AccessKey, credentials2.SecretKey));

        // First call for user 1
        var client1 = await _factory.GetS3ClientAsync(bearerToken1);

        // Act - Second call for different user
        var client2 = await _factory.GetS3ClientAsync(bearerToken2);

        // Assert
        Assert.NotSame(client1, client2);
        _credentialServiceMock.Verify(
                x => x.GetCredentialKeysAsync(bearerToken1),
            Times.Once);
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken2),
            Times.Once);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenTokenMissingOidClaim_ThrowsArgumentException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("preferred_username", TestUserName)
        };

        var key = new SymmetricSecurityKey("test-signing-key-that-is-at-least-32-bytes-long"u8.ToArray());
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var bearerToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _factory.GetS3ClientAsync(bearerToken));

        Assert.Contains("oid claim is missing", exception.Message);
    }

    [Fact]
    public async Task GetS3ClientAsync_WhenCredentialsMoreThan5MinutesRemaining_ReturnsCachedClient()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var validStatus = new CredentialStatus
        {
            Exists = true,
            IsValid = true,
            NeedsRegeneration = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-50),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            RemainingMinutes = 10
        };

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(TestOid))
            .ReturnsAsync(validStatus);

        var firstClient = await _factory.GetS3ClientAsync(bearerToken);

        // Act - Second call should return same cached client
        var secondClient = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.Same(firstClient, secondClient);
        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Once); // Only called once
    }

    [Fact]
    public async Task GetS3ClientAsync_MultipleRefreshCycles_HandlesCorrectly()
    {
        // Arrange
        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);
        var credentials = new S3CredentialsDecrypted
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Metadata = new S3CredentialsMetadata
            {
                UserPrincipalName = TestUserName,
                Salt = "test-salt",
                CreatedAt = DateTime.UtcNow,
                PepperVersion = "v1"
            }
        };

        var callCount = 0;
        _keyVaultServiceMock
            .Setup(x => x.CheckCredentialStatusAsync(TestOid))
            .ReturnsAsync(() =>
            {
                callCount++;
                // First check: credentials still valid
                // Second check: credentials need regeneration
                // Third check: credentials valid again after regeneration
                return new CredentialStatus
                {
                    Exists = true,
                    IsValid = callCount != 2,
                    NeedsRegeneration = callCount == 2,
                    RemainingMinutes = callCount == 2 ? 3 : 55
                };
            });

        _credentialServiceMock
            .Setup(x => x.GetCredentialsAsync(TestOid, TestUserName, bearerToken))
            .ReturnsAsync(credentials);

        // Initial creation
        var client1 = await _factory.GetS3ClientAsync(bearerToken);

        // Second call - credentials still valid
        var client2 = await _factory.GetS3ClientAsync(bearerToken);

        // Third call - credentials need regeneration
        var client3 = await _factory.GetS3ClientAsync(bearerToken);

        // Fourth call - new credentials are valid
        var client4 = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.Same(client1, client2); // Cached client returned
        Assert.NotSame(client2, client3); // New client created due to expiry
        Assert.Same(client3, client4); // Cached client returned after regeneration

        _credentialServiceMock.Verify(
            x => x.GetCredentialKeysAsync(bearerToken),
            Times.Exactly(2));
    }

    [Fact]
    public async Task CreateS3Client_UsesCustomCaValidation_WhenNotDevelopmentAndTrustedCertsPresent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        var trustedCerts = new X509Certificate2Collection { GenerateSelfSignedCertificate("CN=TestCA") };
        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(trustedCerts);

        _netAppCertFactoryMock
            .Setup(f => f.ValidateCertificateWithCustomCa(
                It.IsAny<X509Certificate2?>(),
                It.IsAny<X509Chain?>(),
                It.IsAny<SslPolicyErrors>(),
                It.IsAny<X509Certificate2Collection>()))
            .Returns(true);

        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);

        // Act
        var result = await _factory.GetS3ClientAsync(bearerToken);

        // Assert
        Assert.NotNull(result);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using custom CA certificate validation (non-Development mode)")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateS3Client_CallsValidateCertificateWithCustomCa_WhenNotDevelopmentAndTrustedCertsPresent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        var trustedCerts = new X509Certificate2Collection { GenerateSelfSignedCertificate("CN=TestCA") };
        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(trustedCerts);

        // Act
        await _factory.GetS3ClientAsync(GenerateTestJwtToken(TestOid, TestUserName));

        // Assert
        _netAppCertFactoryMock.Verify(
            f => f.GetTrustedCaCertificates(),
            Times.Once);
    }

    [Fact]
    public async Task CreateS3Client_BypassesSslValidation_WhenIsDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        var trustedCerts = new X509Certificate2Collection();
        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(trustedCerts);

        // Act
        var result = await _factory.GetS3ClientAsync(GenerateTestJwtToken(TestOid, TestUserName));

        // Assert
        Assert.NotNull(result);
        _netAppCertFactoryMock.Verify(
            f => f.ValidateCertificateWithCustomCa(
                It.IsAny<X509Certificate2?>(),
                It.IsAny<X509Chain?>(),
                It.IsAny<SslPolicyErrors>(),
                It.IsAny<X509Certificate2Collection>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateS3Client_BypassesSslValidation_WhenIsDevelopmentWithTrustedCerts()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        // Even with trusted certs present, development mode should bypass SSL
        var trustedCerts = new X509Certificate2Collection { GenerateSelfSignedCertificate("CN=TestCA") };
        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(trustedCerts);

        // Act
        var result = await _factory.GetS3ClientAsync(GenerateTestJwtToken(TestOid, TestUserName));

        // Assert
        Assert.NotNull(result);
        _netAppCertFactoryMock.Verify(
            f => f.ValidateCertificateWithCustomCa(
                It.IsAny<X509Certificate2?>(),
                It.IsAny<X509Chain?>(),
                It.IsAny<SslPolicyErrors>(),
                It.IsAny<X509Certificate2Collection>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateS3Client_ThrowsInvalidOperationException_WhenNotDevelopmentAndNoTrustedCerts()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(new X509Certificate2Collection());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _factory.GetS3ClientAsync(GenerateTestJwtToken(TestOid, TestUserName)));
        Assert.Contains("No trusted CA certificates were loaded", exception.Message);
        Assert.Contains("non-development environments", exception.Message);
    }

    [Fact]
    public async Task CreateS3Client_ThrowsInvalidOperationException_WhenStagingAndNoTrustedCerts()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");

        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(new X509Certificate2Collection());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _factory.GetS3ClientAsync(GenerateTestJwtToken(TestOid, TestUserName)));
    }

    [Fact]
    public async Task GetS3ClientAsync_ThrowsArgumentException_WhenOidClaimMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(new X509Certificate2Collection());

        var handler = new JwtSecurityTokenHandler();
        var tokenWithNoOid = handler.WriteToken(new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: [],
            expires: DateTime.UtcNow.AddHours(1)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _factory.GetS3ClientAsync(tokenWithNoOid));

        Assert.Contains("oid claim is missing", exception.Message);
    }

    [Fact]
    public async Task GetS3ClientAsync_ReturnsCachedClient_WhenOidMatchesAndCredentialsValid()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _netAppCertFactoryMock
            .Setup(f => f.GetTrustedCaCertificates())
            .Returns(new X509Certificate2Collection());

        _keyVaultServiceMock
            .Setup(k => k.CheckCredentialStatusAsync(It.IsAny<string>()))
            .ReturnsAsync(new CredentialStatus { NeedsRegeneration = false });

        var bearerToken = GenerateTestJwtToken(TestOid, TestUserName);

        // Act
        var firstResult = await _factory.GetS3ClientAsync(bearerToken);
        var secondResult = await _factory.GetS3ClientAsync(bearerToken);
        // Assert
        Assert.Same(firstResult, secondResult);
        _credentialServiceMock.Verify(
            s => s.GetCredentialKeysAsync(It.IsAny<string>()),
            Times.Once);
    }

    private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
    {
        var rsa = System.Security.Cryptography.RSA.Create(2048);
        var request = new CertificateRequest(
            subjectName,
            rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1));
    }
}
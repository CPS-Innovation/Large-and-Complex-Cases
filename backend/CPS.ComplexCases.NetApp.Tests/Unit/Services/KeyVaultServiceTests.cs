using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using Azure.Security.KeyVault.Secrets;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using CPS.ComplexCases.NetApp.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Services;

public class KeyVaultServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<SecretClient> _secretClientMock;
    private readonly Mock<ILogger<KeyVaultService>> _loggerMock;
    private readonly KeyVaultService _sut;
    private readonly string _key;

    public KeyVaultServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _secretClientMock = new Mock<SecretClient>();
        _loggerMock = new Mock<ILogger<KeyVaultService>>();
        _sut = new KeyVaultService(_secretClientMock.Object, _loggerMock.Object);

        _key = "test-user";
    }

    [Fact]
    public async Task StoreCredentialsAsync_WithValidCredentials_StoresSuccessfully()
    {
        // Arrange
        var credentials = _fixture.Create<S3CredentialsEncrypted>();
        var expectedSecretValue = JsonSerializer.Serialize(credentials);
        var response = Response.FromValue(
            new KeyVaultSecret($"s3-creds-{_key}", expectedSecretValue),
            Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.SetSecretAsync(It.IsAny<KeyVaultSecret>(), default))
            .ReturnsAsync(response);

        // Act
        await _sut.StoreCredentialsAsync(_key, credentials);

        // Assert
        _secretClientMock.Verify(
            x => x.SetSecretAsync(
                It.Is<KeyVaultSecret>(s =>
                    s.Name == $"s3-creds-{_key}" &&
                    s.Value == expectedSecretValue),
                default),
            Times.Once);
    }

    [Fact]
    public async Task StoreCredentialsAsync_WithValidCredentials_LogsInformation()
    {
        // Arrange
        var credentials = _fixture.Create<S3CredentialsEncrypted>();
        var response = Response.FromValue(
            new KeyVaultSecret($"s3-creds-{_key}", JsonSerializer.Serialize(credentials)),
            Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.SetSecretAsync(It.IsAny<KeyVaultSecret>(), default))
            .ReturnsAsync(response);

        // Act
        await _sut.StoreCredentialsAsync(_key, credentials);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"Stored S3 credentials for user {_key}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StoreCredentialsAsync_WhenExceptionOccurs_ThrowsKeyVaultException()
    {
        // Arrange
        var credentials = _fixture.Create<S3CredentialsEncrypted>();
        var exception = new Exception("Azure error");

        _secretClientMock
            .Setup(x => x.SetSecretAsync(It.IsAny<KeyVaultSecret>(), default))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyVaultException>(() =>
            _sut.StoreCredentialsAsync(_key, credentials));

        Assert.Contains("Failed to store credentials", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }

    [Fact]
    public async Task StoreCredentialsAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var credentials = _fixture.Create<S3CredentialsEncrypted>();
        var exception = new Exception("Azure error");

        _secretClientMock
            .Setup(x => x.SetSecretAsync(It.IsAny<KeyVaultSecret>(), default))
            .ThrowsAsync(exception);

        // Act
        await Assert.ThrowsAsync<KeyVaultException>(() =>
            _sut.StoreCredentialsAsync(_key, credentials));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"Failed to store credentials for user {_key}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithExistingCredentials_ReturnsCredentials()
    {
        // Arrange
        var credentials = _fixture.Create<S3CredentialsEncrypted>();
        var secretValue = JsonSerializer.Serialize(credentials);
        var secret = new KeyVaultSecret($"s3-creds-{_key}", secretValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetCredentialsAsync(_key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(credentials.Metadata.CreatedAt, result.Metadata.CreatedAt);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithNonExistingCredentials_ReturnsNull()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(404, "Not Found");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ThrowsAsync(requestFailedException);

        // Act
        var result = await _sut.GetCredentialsAsync(_key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCredentialsAsync_WithNonExistingCredentials_LogsInformation()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(404, "Not Found");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ThrowsAsync(requestFailedException);

        // Act
        await _sut.GetCredentialsAsync(_key);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"No credentials found for user {_key}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCredentialsAsync_WhenOtherExceptionOccurs_ThrowsKeyVaultException()
    {
        // Arrange
        var exception = new Exception("Unexpected error");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyVaultException>(() =>
            _sut.GetCredentialsAsync(_key));

        Assert.Contains("Failed to retrieve credentials", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }

    [Fact]
    public async Task GetPepperAsync_WithValidVersion_ReturnsPepperValue()
    {
        // Arrange
        var pepperVersion = "v1";
        var pepperValue = "pepper-secret-value";
        var secret = new KeyVaultSecret($"app-pepper-{pepperVersion}", pepperValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"app-pepper-{pepperVersion}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetPepperAsync(pepperVersion);

        // Assert
        Assert.Equal(pepperValue, result);
    }

    [Fact]
    public async Task GetPepperAsync_WithValidVersion_LogsInformation()
    {
        // Arrange
        var pepperVersion = "v1";
        var pepperValue = "pepper-secret-value";
        var secret = new KeyVaultSecret($"app-pepper-{pepperVersion}", pepperValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"app-pepper-{pepperVersion}", null, default))
            .ReturnsAsync(response);

        // Act
        await _sut.GetPepperAsync(pepperVersion);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"Retrieved pepper version {pepperVersion}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPepperAsync_WithNonExistingVersion_ThrowsKeyVaultException()
    {
        // Arrange
        var pepperVersion = "v999";
        var requestFailedException = new RequestFailedException(404, "Not Found");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"app-pepper-{pepperVersion}", null, default))
            .ThrowsAsync(requestFailedException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyVaultException>(() =>
            _sut.GetPepperAsync(pepperVersion));

        Assert.Contains($"Pepper version '{pepperVersion}' not found in Key Vault", ex.Message);
        Assert.Equal(requestFailedException, ex.InnerException);
    }

    [Fact]
    public async Task GetPepperAsync_WhenOtherExceptionOccurs_ThrowsKeyVaultException()
    {
        // Arrange
        var pepperVersion = "v1";
        var exception = new Exception("Unexpected error");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"app-pepper-{pepperVersion}", null, default))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyVaultException>(() =>
            _sut.GetPepperAsync(pepperVersion));

        Assert.Contains("Failed to retrieve pepper version", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }

    [Fact]
    public async Task CheckCredentialStatusAsync_WithNonExistingCredentials_ReturnsNeedsRegenerationStatus()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(404, "Not Found");

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ThrowsAsync(requestFailedException);

        // Act
        var result = await _sut.CheckCredentialStatusAsync(_key);

        // Assert
        Assert.False(result.Exists);
        Assert.False(result.IsValid);
        Assert.True(result.NeedsRegeneration);
    }

    [Fact]
    public async Task CheckCredentialStatusAsync_WithValidCredentials_ReturnsValidStatus()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var credentials = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _fixture.Create<string>(),
            EncryptedSecretKey = _fixture.Create<string>(),
            Metadata = new S3CredentialsMetadata { CreatedAt = createdAt, PepperVersion = "v1", Salt = "salt", UserPrincipalName = "user" }
        };
        var secretValue = JsonSerializer.Serialize(credentials);
        var secret = new KeyVaultSecret($"s3-creds-{_key}", secretValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.CheckCredentialStatusAsync(_key);

        // Assert
        Assert.True(result.Exists);
        Assert.True(result.IsValid);
        Assert.False(result.NeedsRegeneration);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal(createdAt.AddMinutes(60), result.ExpiresAt);
        Assert.True(result.RemainingMinutes > 45);
    }

    [Fact]
    public async Task CheckCredentialStatusAsync_WithExpiredCredentials_ReturnsNeedsRegenerationStatus()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-58); // Only 2 minutes remaining
        var credentials = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _fixture.Create<string>(),
            EncryptedSecretKey = _fixture.Create<string>(),
            Metadata = new S3CredentialsMetadata { CreatedAt = createdAt, PepperVersion = "v1", Salt = "salt", UserPrincipalName = "user" },
        };
        var secretValue = JsonSerializer.Serialize(credentials);
        var secret = new KeyVaultSecret($"s3-creds-{_key}", secretValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.CheckCredentialStatusAsync(_key);

        // Assert
        Assert.True(result.Exists);
        Assert.False(result.IsValid);
        Assert.True(result.NeedsRegeneration);
        Assert.True(result.RemainingMinutes <= 5);
    }

    [Fact]
    public async Task CheckCredentialStatusAsync_WithCredentialsNearExpiry_ReturnsNeedsRegenerationStatus()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-56); // Exactly 4 minutes remaining
        var credentials = new S3CredentialsEncrypted
        {
            EncryptedAccessKey = _fixture.Create<string>(),
            EncryptedSecretKey = _fixture.Create<string>(),
            Metadata = new S3CredentialsMetadata { CreatedAt = createdAt, PepperVersion = "v1", Salt = "salt", UserPrincipalName = "user" }
        };
        var secretValue = JsonSerializer.Serialize(credentials);
        var secret = new KeyVaultSecret($"s3-creds-{_key}", secretValue);
        var response = Response.FromValue(secret, Mock.Of<Response>());

        _secretClientMock
            .Setup(x => x.GetSecretAsync($"s3-creds-{_key}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.CheckCredentialStatusAsync(_key);

        // Assert
        Assert.True(result.Exists);
        Assert.False(result.IsValid); // < 5 minutes remaining
        Assert.True(result.NeedsRegeneration);
    }

}
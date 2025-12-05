namespace CPS.ComplexCases.NetApp.Tests.Unit.Services;

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;
using Microsoft.Extensions.Options;
using Xunit;

public class CryptographyServiceTests
{
    private readonly IFixture _fixture;
    private readonly CryptographyService _sut;
    private readonly CryptoOptions _cryptoOptions;

    public CryptographyServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        // Initialize with default crypto options
        _cryptoOptions = new CryptoOptions();
        _cryptoOptions.Validate();

        var options = Options.Create(_cryptoOptions);
        _sut = new CryptographyService(options);
    }

    [Fact]
    public void CreateSalt_Returns_32_Bytes_And_Is_Random()
    {
        // Act
        var salt1 = _sut.CreateSalt();
        var salt2 = _sut.CreateSalt();

        // Assert
        Assert.NotNull(salt1);
        Assert.Equal(_cryptoOptions.SaltSizeBytes, salt1.Length);
        Assert.NotNull(salt2);
        Assert.Equal(_cryptoOptions.SaltSizeBytes, salt2.Length);

        Assert.NotEqual(Convert.ToBase64String(salt1), Convert.ToBase64String(salt2));
    }

    [Fact]
    public async Task EncryptAsync_Then_DecryptAsync_Roundtrips_Plaintext()
    {
        // Arrange
        var plaintext = "secret-data";
        var objectId = _fixture.Create<string>();
        var pepper = _fixture.Create<string>();
        var salt = _sut.CreateSalt();
        var saltBase64 = Convert.ToBase64String(salt);

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext, objectId, salt, pepper);
        var decrypted = await _sut.DecryptAsync(encrypted, objectId, saltBase64, pepper);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public async Task DecryptAsync_With_Wrong_Pepper_Throws_CryptographicException_Wrapped()
    {
        // Arrange
        var plaintext = "secret-data";
        var objectId = _fixture.Create<string>();
        var pepper = _fixture.Create<string>();
        var wrongPepper = pepper + "_wrong";
        var salt = _sut.CreateSalt();
        var saltBase64 = Convert.ToBase64String(salt);

        var encrypted = await _sut.EncryptAsync(plaintext, objectId, salt, pepper);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<CryptographicException>(() =>
            _sut.DecryptAsync(encrypted, objectId, saltBase64, wrongPepper));

        Assert.Contains("Failed to decrypt credentials. Data may have been tampered with.", ex.Message);
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task DecryptAsync_With_Wrong_ObjectId_Throws_CryptographicException_Wrapped()
    {
        // Arrange
        var plaintext = "another-secret";
        var objectId = _fixture.Create<string>();
        var wrongObjectId = objectId + "_wrong";
        var pepper = _fixture.Create<string>();
        var salt = _sut.CreateSalt();
        var saltBase64 = Convert.ToBase64String(salt);

        var encrypted = await _sut.EncryptAsync(plaintext, objectId, salt, pepper);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<CryptographicException>(() =>
            _sut.DecryptAsync(encrypted, wrongObjectId, saltBase64, pepper));

        Assert.Contains("Failed to decrypt credentials. Data may have been tampered with.", ex.Message);
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task DecryptAsync_With_Wrong_Salt_AAD_Throws_CryptographicException_Wrapped()
    {
        // Arrange
        var plaintext = "sensitive";
        var objectId = _fixture.Create<string>();
        var pepper = _fixture.Create<string>();
        var salt = _sut.CreateSalt();
        var saltBase64 = Convert.ToBase64String(salt);

        var encrypted = await _sut.EncryptAsync(plaintext, objectId, salt, pepper);

        // Create a different salt (same length)
        var differentSalt = _sut.CreateSalt();
        var differentSaltBase64 = Convert.ToBase64String(differentSalt);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<CryptographicException>(() =>
            _sut.DecryptAsync(encrypted, objectId, differentSaltBase64, pepper));

        Assert.Contains("Failed to decrypt credentials. Data may have been tampered with.", ex.Message);
        Assert.NotNull(ex.InnerException);
    }
}
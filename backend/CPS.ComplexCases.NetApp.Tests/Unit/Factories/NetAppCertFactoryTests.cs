using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using Moq;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Factories;

public class NetAppCertFactoryTests
{
    private readonly Mock<ILogger<NetAppCertFactory>> _loggerMock = new();
    private readonly IOptions<NetAppOptions> _options;
    private readonly NetAppCertFactory _factory;
    private const string TestClusterUrl = "https://test-netapp-cluster";
    private const string TestRegionName = "eu-west-1";

    public NetAppCertFactoryTests()
    {
        _options = Options.Create(new NetAppOptions
        {
            RootCaCert = "",
            IssuingCaCert = "",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName,
        });
        _factory = new NetAppCertFactory(_loggerMock.Object, _options);
    }

    [Fact]
    public void GetTrustedCaCertificates_ReturnsCachedCollection_OnSecondCall()
    {
        // Arrange
        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = "",
            IssuingCaCert = "",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act
        var firstCall = factory.GetTrustedCaCertificates();
        var secondCall = factory.GetTrustedCaCertificates();

        // Assert
        Assert.Same(firstCall, secondCall);
    }

    [Fact]
    public void GetTrustedCaCertificates_ReturnsEmptyCollection_WhenNoCertificatesConfigured()
    {
        // Arrange
        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = "",
            IssuingCaCert = "",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act
        var result = factory.GetTrustedCaCertificates();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetTrustedCaCertificates_LoadsRootCaCertificate_WhenProvided()
    {
        // Arrange
        var rootCert = GenerateSelfSignedCertificate("CN=TestRootCA");
        var rootCertBase64 = Convert.ToBase64String(rootCert.Export(X509ContentType.Cert));

        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = rootCertBase64,
            IssuingCaCert = "",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act
        var result = factory.GetTrustedCaCertificates();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(rootCert.Thumbprint, result[0].Thumbprint);
    }

    [Fact]
    public void GetTrustedCaCertificates_LoadsMultipleCertificates_WhenAllProvided()
    {
        // Arrange
        var rootCert = GenerateSelfSignedCertificate("CN=TestRootCA");
        var issuingCert1 = GenerateSelfSignedCertificate("CN=TestIssuingCA1");
        var issuingCert2 = GenerateSelfSignedCertificate("CN=TestIssuingCA2");

        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = Convert.ToBase64String(rootCert.Export(X509ContentType.Cert)),
            IssuingCaCert = Convert.ToBase64String(issuingCert1.Export(X509ContentType.Cert)),
            IssuingCaCert2 = Convert.ToBase64String(issuingCert2.Export(X509ContentType.Cert)),
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act
        var result = factory.GetTrustedCaCertificates();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetTrustedCaCertificates_ThrowsInvalidOperationException_WhenRootCaCertInvalid()
    {
        // Arrange
        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = "invalid-base64-string!!!",
            IssuingCaCert = "",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetTrustedCaCertificates());
        Assert.Contains("Failed to load Root CA certificate from Key Vault", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load Root CA certificate")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetTrustedCaCertificates_ThrowsInvalidOperationException_WhenIssuingCaCertInvalid()
    {
        // Arrange
        var rootCert = GenerateSelfSignedCertificate("CN=TestRootCA");
        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = Convert.ToBase64String(rootCert.Export(X509ContentType.Cert)),
            IssuingCaCert = "invalid-base64-string!!!",
            IssuingCaCert2 = "",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetTrustedCaCertificates());
        Assert.Contains("Failed to load Issuing CA certificate from Key Vault", exception.Message);
    }

    [Fact]
    public void GetTrustedCaCertificates_ThrowsInvalidOperationException_WhenIssuingCaCert2Invalid()
    {
        // Arrange
        var rootCert = GenerateSelfSignedCertificate("CN=TestRootCA");
        var issuingCert = GenerateSelfSignedCertificate("CN=TestIssuingCA");
        var options = Options.Create(new NetAppOptions
        {
            RootCaCert = Convert.ToBase64String(rootCert.Export(X509ContentType.Cert)),
            IssuingCaCert = Convert.ToBase64String(issuingCert.Export(X509ContentType.Cert)),
            IssuingCaCert2 = "invalid-base64-string!!!",
            Url = TestClusterUrl,
            RegionName = TestRegionName
        });
        var factory = new NetAppCertFactory(_loggerMock.Object, options);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetTrustedCaCertificates());
        Assert.Contains("Failed to load Issuing CA certificate 2 from Key Vault", exception.Message);
    }

    [Fact]
    public void ValidateCertificateWithCustomCa_ReturnsTrue_WhenSslPolicyErrorsIsNone()
    {
        // Arrange
        var certificate = GenerateSelfSignedCertificate("CN=TestCert");
        var trustedCerts = new X509Certificate2Collection();

        // Act
        var result = _factory.ValidateCertificateWithCustomCa(certificate, new X509Chain(), SslPolicyErrors.None, trustedCerts);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateCertificateWithCustomCa_ReturnsFalse_WhenCertificateIsNull()
    {
        // Arrange
        var trustedCerts = new X509Certificate2Collection();

        // Act
        var result = _factory.ValidateCertificateWithCustomCa(null, new X509Chain(), SslPolicyErrors.RemoteCertificateNameMismatch, trustedCerts);

        // Assert
        Assert.False(result);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Certificate or chain is null")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ValidateCertificateWithCustomCa_ReturnsFalse_WhenChainIsNull()
    {
        // Arrange
        var certificate = GenerateSelfSignedCertificate("CN=TestCert");
        var trustedCerts = new X509Certificate2Collection();

        // Act
        var result = _factory.ValidateCertificateWithCustomCa(certificate, null, SslPolicyErrors.RemoteCertificateNameMismatch, trustedCerts);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateCertificateWithCustomCa_LogsInformation_WhenValidatingCertificate()
    {
        // Arrange
        var certificate = GenerateSelfSignedCertificate("CN=TestCert");
        var trustedCerts = new X509Certificate2Collection();

        // Act
        _factory.ValidateCertificateWithCustomCa(certificate, new X509Chain(), SslPolicyErrors.RemoteCertificateChainErrors, trustedCerts);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SSL policy errors detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper method to generate self-signed certificates for testing
    private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
    {
        var rsa = System.Security.Cryptography.RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1)
        );
    }
}
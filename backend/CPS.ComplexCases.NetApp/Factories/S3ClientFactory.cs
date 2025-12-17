using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.NetApp.Factories;

public class S3ClientFactory(IOptions<NetAppOptions> options, IS3CredentialService s3CredentialService, ILogger<S3ClientFactory> logger) : IS3ClientFactory
{
    private readonly ILogger<S3ClientFactory> _logger  = logger;
    private readonly NetAppOptions _options = options.Value;
    private readonly IS3CredentialService _s3CredentialsService = s3CredentialService;
    private IAmazonS3? _s3Client;
    private X509Certificate2Collection? _trustedCaCertificates;

    public async Task<IAmazonS3> GetS3ClientAsync(string bearerToken)
    {
        if (_s3Client == null)
        {
            _s3Client = await CreateS3Client(bearerToken);
        }

        return _s3Client;
    }

    public void SetS3ClientAsync(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    private async Task<IAmazonS3> CreateS3Client(string bearerToken)
    {
        var (accessKey, secretKey) = await GetCredentialKeysAsync(bearerToken);
        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.RegionName),
            ServiceURL = _options.Url,
            ForcePathStyle = true,
            LogMetrics = true,
            LogResponse = true,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
        };

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (isDevelopment)
        {
            // In Development, bypass all SSL validation
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };

            s3Config.HttpClientFactory = new CustomHttpClientFactory(handler);
        }
        else
        {
            // In non-Development environments, load and trust the custom CA certificates
            var trustedCerts = GetTrustedCaCertificates();
            if (trustedCerts.Count > 0)
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                        ValidateCertificateWithCustomCa(cert, chain, sslPolicyErrors, trustedCerts)
                };

                s3Config.HttpClientFactory = new CustomHttpClientFactory(handler);
            }
        }

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var s3Client = new AmazonS3Client(credentials, s3Config);

        return s3Client;
    }

    private X509Certificate2Collection GetTrustedCaCertificates()
    {
        if (_trustedCaCertificates != null)
        {
            return _trustedCaCertificates;
        }

        _trustedCaCertificates = new X509Certificate2Collection();

        // Load Root CA certificate from environment variable (Base64 encoded)
        var rootCaBase64 = _options.RootCaCert;
        if (!string.IsNullOrEmpty(rootCaBase64))
        {
            try
            {
                var rootCaBytes = Convert.FromBase64String(rootCaBase64);
                var rootCaCert = new X509Certificate2(rootCaBytes);
                _trustedCaCertificates.Add(rootCaCert);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load Root CA certificate from KV. Error: {Ex}.", ex);
            }
        }

        // Load Issuing CA certificate from environment variable (Base64 encoded)
        var issuingCaBase64 = _options.IssuingCaCert;
        if (!string.IsNullOrEmpty(issuingCaBase64))
        {
            try
            {
                var issuingCaBytes = Convert.FromBase64String(issuingCaBase64);
                var issuingCaCert = new X509Certificate2(issuingCaBytes);
                _trustedCaCertificates.Add(issuingCaCert);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load Issuing CA certificate from KV. Error: {Ex}.", ex);
            }
        }

        // Load second Issuing CA certificate from environment variable (Base64 encoded)
        var issuingCa2Base64 = _options.IssuingCaCert2;
        if (!string.IsNullOrEmpty(issuingCa2Base64))
        {
            try
            {
                var issuingCa2Bytes = Convert.FromBase64String(issuingCa2Base64);
                var issuingCa2Cert = new X509Certificate2(issuingCa2Bytes);
                _trustedCaCertificates.Add(issuingCa2Cert);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load Issuing CA certificate 2 from KV. Error: {Ex}.", ex);
            }
        }

        return _trustedCaCertificates;
    }

    private static bool ValidateCertificateWithCustomCa(
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors,
        X509Certificate2Collection trustedCaCerts)
    {
        // If no errors, certificate is valid
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        // If the only error is untrusted root, try validating with our custom CAs
        if (certificate == null || chain == null)
        {
            return false;
        }

        // Create a new chain with custom trust settings
        using var customChain = new X509Chain();
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

        // Add our trusted CA certificates to the extra store
        foreach (var caCert in trustedCaCerts)
        {
            customChain.ChainPolicy.ExtraStore.Add(caCert);
        }

        // Build the chain
        var isChainValid = customChain.Build(certificate);

        if (!isChainValid)
        {
            return false;
        }

        // Verify that the chain ends with one of our trusted CAs
        var chainRoot = customChain.ChainElements[^1].Certificate;
        foreach (var trustedCa in trustedCaCerts)
        {
            if (chainRoot.Thumbprint == trustedCa.Thumbprint)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<(string? accessKey, string? secretKey)> GetCredentialKeysAsync(string bearerToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(bearerToken);

        var username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value.ToLowerInvariant();
        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username), "preferred_username claim is missing in the bearer token.");
        }

        if (string.IsNullOrEmpty(oid))
        {
            throw new ArgumentNullException(nameof(oid), "oid claim is missing in the bearer token.");
        }

        var credentials = await _s3CredentialsService.GetCredentialsAsync(oid, username, bearerToken);

        return (credentials.AccessKey, credentials.SecretKey);
    }
}

public class CustomHttpClientFactory(HttpClientHandler handler) : Amazon.Runtime.HttpClientFactory
{
    private readonly HttpClientHandler _handler = handler;

    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return new HttpClient(_handler);
    }
}
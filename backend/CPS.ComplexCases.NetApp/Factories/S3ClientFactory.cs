using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;
using CPS.ComplexCases.NetApp.Telemetry;

namespace CPS.ComplexCases.NetApp.Factories;

public class S3ClientFactory(
    IOptions<NetAppOptions> options,
    IS3CredentialService s3CredentialService,
    IKeyVaultService keyVaultService,
    ILogger<S3ClientFactory> logger,
    IS3TelemetryHandler telemetryHandler) : IS3ClientFactory
{
    private readonly ILogger<S3ClientFactory> _logger = logger;
    private readonly IS3TelemetryHandler _telemetryHandler = telemetryHandler;
    private readonly NetAppOptions _options = options.Value;
    private readonly IS3CredentialService _s3CredentialsService = s3CredentialService;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;
    private IAmazonS3? _s3Client;
    private string? _currentOid;
    private X509Certificate2Collection? _trustedCaCertificates;

    public async Task<IAmazonS3> GetS3ClientAsync(string bearerToken)
    {
        var oid = ExtractOidFromToken(bearerToken);

        if (_s3Client != null && _currentOid == oid)
        {
            // Check if credentials are still valid before returning cached client
            var status = await _keyVaultService.CheckCredentialStatusAsync(oid);

            if (!status.NeedsRegeneration)
            {
                return _s3Client;
            }

            _logger.LogInformation(
                "Credentials expiring soon for user {Oid} ({RemainingMinutes:F1} minutes remaining) - recreating S3 client",
                oid,
                status.RemainingMinutes);
        }

        _currentOid = oid;
        _s3Client = await CreateS3Client(bearerToken);
        return _s3Client;
    }

    private string ExtractOidFromToken(string bearerToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(bearerToken);
        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new ArgumentException("oid claim is missing in the bearer token.", nameof(bearerToken));
        }

        return oid;
    }

    public void SetS3ClientAsync(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    private async Task<IAmazonS3> CreateS3Client(string bearerToken)
    {
        var (accessKey, secretKey) = await _s3CredentialsService.GetCredentialKeysAsync(bearerToken);
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
            _logger.LogInformation("Using custom CA certificate validation (non-Development mode).");
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

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        var s3Client = new AmazonS3Client(credentials, s3Config);

        s3Client.BeforeRequestEvent += (sender, args) =>
        {
            var webServiceArgs = args as WebServiceRequestEventArgs;
            _telemetryHandler.InitiateTelemetryEvent(webServiceArgs);
        };

        s3Client.AfterResponseEvent += (sender, args) =>
        {
            var webServiceArgs = args as WebServiceResponseEventArgs;
            _telemetryHandler.CompleteTelemetryEvent(webServiceArgs);
        };

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
                _logger.LogError(ex, "Failed to load Root CA certificate from KV. Error: {Message}.", ex.Message);
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
                _logger.LogError(ex, "Failed to load Issuing CA certificate from KV. Error: {Message}.", ex.Message);
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
                _logger.LogError(ex, "Failed to load Issuing CA certificate 2 from KV. Error: {Message}.", ex.Message);
            }
        }

        return _trustedCaCertificates;
    }

    private bool ValidateCertificateWithCustomCa(
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

        _logger.LogInformation("SSL policy errors detected: {SslPolicyErrors}. Attempting custom CA validation.", sslPolicyErrors);

        // If the only error is untrusted root, try validating with our custom CAs
        if (certificate == null || chain == null)
        {
            _logger.LogError("Certificate or chain is null. Cannot validate.");
            return false;
        }

        _logger.LogInformation("Validating certificate: Subject={Subject}, Issuer={Issuer}",
            certificate.Subject, certificate.Issuer);
        _logger.LogInformation("Trusted CA certificates count: {Count}", trustedCaCerts.Count);

        // Create a new chain with custom trust settings
        using var customChain = new X509Chain();

        // SECURITY NOTE: Revocation checking is disabled for the following reasons:
        // 1. Internal CA certificates typically do not have publicly accessible CRL/OCSP endpoints
        // 2. The NetApp storage service uses an internal CA without revocation infrastructure
        // COMPENSATING CONTROLS:
        // - Explicit thumbprint verification against known trusted CA certificates (lines 214-221)
        // - CA certificates are securely stored in Azure Key Vault
        // - Connection restricted to specific internal service endpoint via _options.Url
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            | X509VerificationFlags.IgnoreEndRevocationUnknown
            | X509VerificationFlags.IgnoreCtlSignerRevocationUnknown
            | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
            | X509VerificationFlags.IgnoreRootRevocationUnknown;

        // Add our trusted CA certificates to the extra store
        foreach (var caCert in trustedCaCerts)
        {
            _logger.LogInformation("Adding trusted CA to chain: Subject={Subject}, Thumbprint={Thumbprint}",
                caCert.Subject, caCert.Thumbprint);
            customChain.ChainPolicy.ExtraStore.Add(caCert);
        }

        // Build the chain
        var isChainValid = customChain.Build(certificate);

        if (!isChainValid)
        {
            foreach (var status in customChain.ChainStatus)
            {
                _logger.LogError("Chain validation failed: Status={Status}, StatusInformation={StatusInformation}",
                    status.Status, status.StatusInformation);
            }
            return false;
        }

        // Verify that the chain ends with one of our trusted CAs
        var chainRoot = customChain.ChainElements[^1].Certificate;
        _logger.LogInformation("Chain root certificate: Subject={Subject}, Thumbprint={Thumbprint}",
            chainRoot.Subject, chainRoot.Thumbprint);

        foreach (var trustedCa in trustedCaCerts)
        {
            if (chainRoot.Thumbprint == trustedCa.Thumbprint)
            {
                _logger.LogInformation("Certificate validated successfully against trusted CA: {Subject}", trustedCa.Subject);
                return true;
            }
        }

        _logger.LogError("Chain root thumbprint {ChainRootThumbprint} did not match any trusted CA thumbprints.",
            chainRoot.Thumbprint);
        return false;
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
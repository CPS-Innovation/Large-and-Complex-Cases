using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
    IS3TelemetryHandler telemetryHandler,
    INetAppCertFactory netAppCertFactory) : IS3ClientFactory
{
    private readonly ILogger<S3ClientFactory> _logger = logger;
    private readonly IS3TelemetryHandler _telemetryHandler = telemetryHandler;
    private readonly INetAppCertFactory _netAppCertFactory = netAppCertFactory;
    private readonly NetAppOptions _options = options.Value;
    private readonly IS3CredentialService _s3CredentialsService = s3CredentialService;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;
    private IAmazonS3? _s3Client;
    private string? _currentOid;
    // Serialises concurrent credential checks and rotations so that only one task
    // can rotate credentials at a time. Without this, multiple concurrent chunk
    // uploads can each detect expiry and independently call RegenerateUserKeysAsync,
    // causing the old keys to be invalidated while sibling uploads are still in flight.
    private readonly SemaphoreSlim _clientLock = new(1, 1);

    public async Task<IAmazonS3> GetS3ClientAsync(string bearerToken)
    {
        var oid = ExtractOidFromToken(bearerToken);

        await _clientLock.WaitAsync();
        try
        {
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
        finally
        {
            _clientLock.Release();
        }
    }

    private static string ExtractOidFromToken(string bearerToken)
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
            var trustedCerts = _netAppCertFactory.GetTrustedCaCertificates();
            if (trustedCerts.Count > 0)
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                        _netAppCertFactory.ValidateCertificateWithCustomCa(cert, chain, sslPolicyErrors, trustedCerts)
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
}

public class CustomHttpClientFactory(HttpClientHandler handler) : Amazon.Runtime.HttpClientFactory
{
    private readonly HttpClientHandler _handler = handler;

    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return new HttpClient(_handler);
    }
}
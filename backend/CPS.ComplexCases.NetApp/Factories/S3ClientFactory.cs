using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.Extensions.Options;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;

namespace CPS.ComplexCases.NetApp.Factories;

public class S3ClientFactory(IOptions<NetAppOptions> options, IS3CredentialService s3CredentialService) : IS3ClientFactory
{
    private readonly NetAppOptions _options = options.Value;
    private readonly IS3CredentialService _s3CredentialsService = s3CredentialService;
    private IAmazonS3? _s3Client;

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

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        var customHttpClientFactory = new CustomHttpClientFactory(handler);

        var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.RegionName),
            ServiceURL = _options.Url,
            ForcePathStyle = true,
            LogMetrics = true,
            LogResponse = true,
            HttpClientFactory = customHttpClientFactory, // Remove?
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
        });

        return s3Client;
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
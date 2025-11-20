using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.Extensions.Options;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Factories;

public class S3ClientFactory(INetAppHttpClient netAppHttpClient, INetAppArgFactory netAppArgFactory, IOptions<NetAppOptions> options, IUserService userService) : IS3ClientFactory
{
    private readonly INetAppHttpClient _netAppHttpClient = netAppHttpClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly NetAppOptions _options = options.Value;
    private readonly IUserService _userService = userService;
    private IAmazonS3? _s3Client;

    public async Task<IAmazonS3> GetS3ClientAsync()
    {
        if (_s3Client == null)
        {
            _s3Client = await CreateS3Client();
        }

        return _s3Client;
    }

    public void SetS3ClientAsync(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    private async Task<IAmazonS3> CreateS3Client()
    {
        var bearerToken = await _userService.GetUserBearerTokenAsync();
        var (accessKey, secretKey) = await GetCredentialKeysAsync(bearerToken!);
        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
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

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username), "preferred_username claim is missing in the bearer token.");
        }

        NetAppUserResponse userResponse = new();

        try
        {
            userResponse = await _netAppHttpClient.RegenerateUserKeysAsync(_netAppArgFactory.CreateRegenerateUserKeysArg(username, bearerToken, _options.SecurityGroupId));
        }
        catch (NetAppNotFoundException)
        {
            userResponse = await _netAppHttpClient.RegisterUserAsync(_netAppArgFactory.CreateRegisterUserArg(username, bearerToken, _options.SecurityGroupId));
        }
        catch (NetAppClientException)
        {
            throw;
        }

        return (userResponse.Records.FirstOrDefault()?.AccessKey, userResponse.Records.FirstOrDefault()?.SecretKey);
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
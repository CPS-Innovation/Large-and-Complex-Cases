using System.Net;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CPS.ComplexCases.API.Integration.Tests.Configuration;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Data;
using CPS.ComplexCases.DDEI;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Tactical.Client;
using CPS.ComplexCases.DDEI.Tactical.Mappers;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;
using CPS.ComplexCases.NetApp.Telemetry;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;
using Polly.Retry;

namespace CPS.ComplexCases.API.Integration.Tests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly ILoggerFactory _loggerFactory;
    private NpgsqlDataSource? _dbDataSource;
    private string? _cachedBearerToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    public IConfiguration Configuration { get; }
    public IntegrationTestSettings Settings { get; }

    public bool IsEgressConfigured => Settings.Egress.IsConfigured;
    public bool IsDdeiConfigured => Settings.DDEI.IsConfigured;
    public bool IsDdeiAuthConfigured => Settings.DDEI.IsAuthConfigured;
    public bool IsDatabaseConfigured => !string.IsNullOrEmpty(Settings.CaseManagementDatastoreConnection);

    public bool IsNetAppConfigured => Settings.NetApp.IsConfigured && Settings.KeyVault.IsConfigured &&
                                      Settings.Azure.IsUserAuthConfigured;

    public bool IsFullyConfigured => IsEgressConfigured && IsDdeiConfigured && IsDatabaseConfigured;

    public EgressStorageClient? EgressStorageClient { get; private set; }
    public EgressClient? EgressClient { get; private set; }
    public IDdeiClient? DdeiClient { get; private set; }
    public IDdeiClientTactical? DdeiClientTactical { get; private set; }
    public ApplicationDbContext? DbContext { get; private set; }
    public INetAppClient? NetAppClient { get; private set; }
    public INetAppArgFactory? NetAppArgFactory { get; private set; }
    public INetAppS3HttpClient? NetAppS3HttpClient { get; private set; }
    public INetAppS3HttpArgFactory? NetAppS3HttpArgFactory { get; private set; }

    public string? EgressWorkspaceId => Settings.Egress.WorkspaceId;
    public int? DdeiTestCaseId => Settings.DDEI.TestCaseId;
    public string? NetAppBucketName => Settings.NetApp.BucketName;
    public string? NetAppTestFolderPrefix => Settings.NetApp.TestFolderPrefix;

    public IntegrationTestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        Settings = IntegrationTestSettings.FromConfiguration(Configuration);

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }

    public Task InitializeAsync()
    {
        InitializeEgressClients();
        InitializeDdeiClient();
        InitializeDbContext();
        InitializeNetAppClient();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        DbContext?.Dispose();
        _dbDataSource?.Dispose();
        _loggerFactory.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a user-delegated bearer token using ROPC (Resource Owner Password Credentials) flow.
    /// This token contains preferred_username and oid claims required for NetApp S3 credential service.
    /// Uses UserScope (e.g., api://{client-id}/user_impersonation) instead of .default scope.
    /// </summary>
    public async Task<string> GetUserDelegatedBearerTokenAsync()
    {
        if (!Settings.Azure.IsUserAuthConfigured)
        {
            throw new InvalidOperationException(
                "Azure AD user auth settings not configured. Set Azure__TenantId, Azure__ClientId, Azure__ClientSecret, Azure__UserScope, Azure__TestUserEmail, and Azure__TestUserPassword environment variables.");
        }

        // Return cached token if still valid (with 5 minute buffer)
        if (_cachedBearerToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedBearerToken;
        }

        var credential = new UsernamePasswordCredential(
            Settings.Azure.TestUserEmail,
            Settings.Azure.TestUserPassword,
            Settings.Azure.TenantId,
            Settings.Azure.ClientId);

        var tokenRequestContext = new TokenRequestContext(new[] { Settings.Azure.UserScope! });
        var token = await credential.GetTokenAsync(tokenRequestContext);

        _cachedBearerToken = token.Token;
        _tokenExpiry = token.ExpiresOn.UtcDateTime;

        return _cachedBearerToken;
    }

    /// <summary>
    /// Gets CMS auth values (JSON containing Cookies and Token) from DDEI authentication.
    /// The returned string should be passed as the CmsAuthValues parameter to DdeiArgFactory.
    /// </summary>
    public async Task<string> GetCmsAuthValuesAsync()
    {
        if (!IsDdeiAuthConfigured || DdeiClientTactical == null)
        {
            throw new InvalidOperationException("DDEI auth settings not configured.");
        }

        var response = await DdeiClientTactical.AuthenticateAsync(
            Settings.DDEI.Username!,
            Settings.DDEI.Password!);

        // DDEI expects the Cms-Auth-Values header to be a JSON object containing both Cookies and Token
        return System.Text.Json.JsonSerializer.Serialize(new { response.Cookies, response.Token });
    }

    private void InitializeEgressClients()
    {
        if (!IsEgressConfigured) return;

        var egressOptions = new EgressOptions
        {
            Url = Settings.Egress.Url!,
            Username = Settings.Egress.Username!,
            Password = Settings.Egress.Password!
        };

        var optionsWrapper = new OptionsWrapper<EgressOptions>(egressOptions);

        // Use retry handler to handle 429 (Too Many Requests) from Egress API
        var httpClient = new HttpClient(new RetryDelegatingHandler())
        {
            BaseAddress = new Uri(egressOptions.Url)
        };

        var requestFactory = new EgressRequestFactory();
        var telemetryClient = new TelemetryClientStub();

        var storageLogger = _loggerFactory.CreateLogger<EgressStorageClient>();
        EgressStorageClient = new EgressStorageClient(
            storageLogger,
            optionsWrapper,
            httpClient,
            requestFactory,
            telemetryClient);

        var clientLogger = _loggerFactory.CreateLogger<EgressClient>();
        var clientHttpClient = new HttpClient(new RetryDelegatingHandler())
        {
            BaseAddress = new Uri(egressOptions.Url)
        };
        EgressClient = new EgressClient(
            clientLogger,
            optionsWrapper,
            clientHttpClient,
            requestFactory,
            telemetryClient);
    }

    private void InitializeDdeiClient()
    {
        if (!IsDdeiConfigured) return;

        var ddeiOptions = new DDEIOptions
        {
            BaseUrl = Settings.DDEI.BaseUrl!,
            AccessKey = Settings.DDEI.AccessKey!,
            MockBaseUrl = string.Empty,
            DevtunnelToken = string.Empty
        };

        var optionsWrapper = new OptionsWrapper<DDEIOptions>(ddeiOptions);

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ddeiOptions.BaseUrl)
        };
        httpClient.DefaultRequestHeaders.Add("X-Functions-Key", ddeiOptions.AccessKey);
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

        var mockSwitch = new MockSwitch(optionsWrapper);
        var ddeiLogger = _loggerFactory.CreateLogger<DdeiClient>();
        var requestFactory = new DdeiRequestFactory(mockSwitch);
        var argFactory = new DdeiArgFactory();
        var caseDetailsMapper = new CaseDetailsMapper();
        var areasMapper = new AreasMapper();
        var tacticalRequestFactory = new DdeiRequestFactoryTactical(mockSwitch);
        var authResponseMapper = new AuthenticationResponseMapper();
        var telemetryClient = new TelemetryClientStub();

        var ddeiClient = new DdeiClient(
            ddeiLogger,
            httpClient,
            requestFactory,
            argFactory,
            caseDetailsMapper,
            areasMapper,
            tacticalRequestFactory,
            authResponseMapper,
            telemetryClient);

        DdeiClient = ddeiClient;
        DdeiClientTactical = ddeiClient;
    }

    private void InitializeDbContext()
    {
        if (!IsDatabaseConfigured) return;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var authMode = Configuration["Postgres:AuthMode"];

        if (authMode?.Equals("AAD", StringComparison.OrdinalIgnoreCase) == true)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(Settings.CaseManagementDatastoreConnection);

            if (dataSourceBuilder.ConnectionStringBuilder.Password is null)
            {
                var dbUserName = Configuration["Postgres:DbUserName"];
                if (!string.IsNullOrEmpty(dbUserName))
                {
                    dataSourceBuilder.ConnectionStringBuilder.Username = dbUserName;
                }

                var credentials = new DefaultAzureCredential();
                dataSourceBuilder.UsePeriodicPasswordProvider(
                    async (_, ct) =>
                    {
                        var token = await credentials.GetTokenAsync(
                            new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                            ct).ConfigureAwait(false);
                        return token.Token;
                    },
                    TimeSpan.FromHours(1),
                    TimeSpan.FromSeconds(30));
            }

            _dbDataSource = dataSourceBuilder.Build();
            optionsBuilder.UseNpgsql(_dbDataSource, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory", Data.Constants.SchemaNames.Lcc));
        }
        else
        {
            optionsBuilder.UseNpgsql(Settings.CaseManagementDatastoreConnection, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory", Data.Constants.SchemaNames.Lcc));
        }

        DbContext = new ApplicationDbContext(optionsBuilder.Options);
    }

    private void InitializeNetAppClient()
    {
        if (!IsNetAppConfigured) return;

        // Set Development environment to bypass SSL validation in S3ClientFactory
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        var netAppOptions = new NetAppOptions
        {
            Url = Settings.NetApp.Url!,
            RegionName = Settings.NetApp.RegionName!,
            S3ServiceUuid = Settings.NetApp.S3ServiceUuid,
            SessionDurationSeconds = Settings.NetApp.SessionDurationSeconds,
            PepperVersion = Settings.NetApp.PepperVersion
        };

        var netAppOptionsWrapper = new OptionsWrapper<NetAppOptions>(netAppOptions);

        var cryptoOptions = new CryptoOptions();
        var cryptoOptionsWrapper = new OptionsWrapper<CryptoOptions>(cryptoOptions);

        var credential = new ClientSecretCredential(
            Settings.Azure.TenantId,
            Settings.Azure.ClientId,
            Settings.Azure.ClientSecret);

        var secretClient = new SecretClient(new Uri(Settings.KeyVault.Url!), credential);

        var keyVaultServiceLogger = _loggerFactory.CreateLogger<KeyVaultService>();
        var keyVaultService =
            new KeyVaultService(secretClient, keyVaultServiceLogger, netAppOptions.SessionDurationSeconds);

        var cryptographyService = new CryptographyService(cryptoOptionsWrapper);

        // Create NetApp HTTP client for user registration/key regeneration
        // Bypass SSL validation for integration tests (cluster uses self-signed/internal CA certificates)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var netAppHttpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(Settings.NetApp.ClusterUrl!)
        };
        var netAppRequestFactory = new NetAppRequestFactory();
        var netAppHttpClientLogger = _loggerFactory.CreateLogger<NetAppHttpClient>();
        var netAppClient = new NetAppHttpClient(netAppHttpClient, netAppRequestFactory, netAppHttpClientLogger);

        NetAppArgFactory = new NetAppArgFactory();
        var netAppArgFactory = new NetAppArgFactory();

        var s3CredentialServiceLogger = _loggerFactory.CreateLogger<S3CredentialService>();
        var s3CredentialService = new S3CredentialService(
            keyVaultService,
            netAppClient,
            netAppArgFactory,
            cryptographyService,
            netAppOptionsWrapper,
            s3CredentialServiceLogger);

        var netAppCertFactoryLogger = _loggerFactory.CreateLogger<NetAppCertFactory>();
        var netAppCertFactory = new NetAppCertFactory(netAppCertFactoryLogger, netAppOptionsWrapper);

        var s3ClientFactoryLogger = _loggerFactory.CreateLogger<S3ClientFactory>();
        var s3TelemetryHandler = new S3TelemetryHandlerStub();
        var s3ClientFactory = new S3ClientFactory(
            netAppOptionsWrapper,
            s3CredentialService,
            keyVaultService,
            s3ClientFactoryLogger,
            s3TelemetryHandler,
            netAppCertFactory);

        // Create NetApp S3 HTTP client for HeadObject/verify operations
        var netAppS3HttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var netAppS3HttpClientHttp = new HttpClient(netAppS3HttpHandler)
        {
            BaseAddress = new Uri(Settings.NetApp.Url!)
        };
        NetAppS3HttpClient = new NetAppS3HttpClient(netAppS3HttpClientHttp, s3CredentialService, netAppOptionsWrapper);
        NetAppS3HttpArgFactory = new NetAppS3HttpArgFactory();

        var amazonS3UtilsWrapper = new AmazonS3UtilsWrapper();

        var netAppClientLogger = _loggerFactory.CreateLogger<NetAppClient>();
        NetAppClient = new NetAppClient(
            netAppClientLogger,
            amazonS3UtilsWrapper,
            netAppRequestFactory,
            s3ClientFactory,
            NetAppS3HttpClient,
            NetAppS3HttpArgFactory);
    }
}

/// <summary>
/// Stub implementation of IS3TelemetryHandler for integration tests
/// </summary>
public class S3TelemetryHandlerStub : IS3TelemetryHandler
{
    public void InitiateTelemetryEvent(Amazon.Runtime.WebServiceRequestEventArgs? args)
    {
    }

    public void CompleteTelemetryEvent(Amazon.Runtime.WebServiceResponseEventArgs? args)
    {
    }
}

/// <summary>
/// Delegating handler that retries requests on 429 (Too Many Requests) and 5xx responses
/// with exponential backoff. Used for integration tests to handle Egress rate limiting.
/// </summary>
public class RetryDelegatingHandler : DelegatingHandler
{
    // Shared concurrency limiter across all handler instances — mirrors the production
    // bulkhead policy (maxParallelization: 30) to prevent flooding the Egress API
    // and triggering sustained 429s during recursive parallel folder traversal.
    private static readonly SemaphoreSlim _concurrencyLimiter = new(10, 10);

    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public RetryDelegatingHandler() : base(new HttpClientHandler())
    {
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response =>
                        response.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500),
                DelayGenerator = static args =>
                {
                    // Respect Retry-After header from 429 responses if present
                    if (args.Outcome.Result is HttpResponseMessage { StatusCode: HttpStatusCode.TooManyRequests } response
                        && response.Headers.RetryAfter?.Delta is TimeSpan retryAfter)
                    {
                        return new ValueTask<TimeSpan?>(retryAfter);
                    }
                    return new ValueTask<TimeSpan?>((TimeSpan?)null);
                }
            })
            .Build();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var clonedRequest = await CloneHttpRequestMessageAsync(request);
                return await base.SendAsync(clonedRequest, token);
            }, cancellationToken);
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var property in request.Options)
        {
            clone.Options.TryAdd(property.Key, property.Value);
        }

        return clone;
    }
}
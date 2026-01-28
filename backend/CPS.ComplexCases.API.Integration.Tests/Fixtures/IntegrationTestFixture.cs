using Azure.Core;
using Azure.Identity;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Integration.Tests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly ILoggerFactory _loggerFactory;
    private string? _cachedBearerToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    public IConfiguration Configuration { get; }
    public IntegrationTestSettings Settings { get; }

    public bool IsEgressConfigured => Settings.Egress.IsConfigured;
    public bool IsDdeiConfigured => Settings.DDEI.IsConfigured;
    public bool IsDdeiAuthConfigured => Settings.DDEI.IsAuthConfigured;
    public bool IsDatabaseConfigured => !string.IsNullOrEmpty(Settings.CaseManagementDatastoreConnection);
    public bool IsFullyConfigured => IsEgressConfigured && IsDdeiConfigured && IsDatabaseConfigured;

    public EgressStorageClient? EgressStorageClient { get; private set; }
    public EgressClient? EgressClient { get; private set; }
    public IDdeiClient? DdeiClient { get; private set; }
    public IDdeiClientTactical? DdeiClientTactical { get; private set; }
    public ApplicationDbContext? DbContext { get; private set; }

    public string? EgressWorkspaceId => Settings.Egress.WorkspaceId;
    public int? DdeiTestCaseId => Settings.DDEI.TestCaseId;

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

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        DbContext?.Dispose();
        _loggerFactory.Dispose();
        return Task.CompletedTask;
    }

    public async Task<string> GetAzureAdBearerTokenAsync()
    {
        if (!Settings.Azure.IsConfigured)
        {
            throw new InvalidOperationException("Azure AD settings not configured. Set Azure__TenantId, Azure__ClientId, Azure__ClientSecret, and Azure__Scope environment variables.");
        }

        // Return cached token if still valid (with 5 minute buffer)
        if (_cachedBearerToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedBearerToken;
        }

        var credential = new ClientSecretCredential(
            Settings.Azure.TenantId,
            Settings.Azure.ClientId,
            Settings.Azure.ClientSecret);

        var tokenRequestContext = new TokenRequestContext(new[] { Settings.Azure.Scope! });
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
        var httpClient = new HttpClient
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
        var clientHttpClient = new HttpClient
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
        optionsBuilder.UseNpgsql(Settings.CaseManagementDatastoreConnection);

        DbContext = new ApplicationDbContext(optionsBuilder.Options);
    }
}

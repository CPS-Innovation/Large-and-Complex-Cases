using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ThrottlingTroll;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Middleware;
using CPS.ComplexCases.API.OpenApi;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.DDEI.Extensions;
using CPS.ComplexCases.DDEI.Tactical.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.NetApp.Extensions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

static string ExtractIdentityFromRequest(IReadOnlyDictionary<string, Microsoft.Extensions.Primitives.StringValues> headers)
{
    try
    {
        // User identity from Bearer token if available
        if (headers.TryGetValue("Authorization", out var authHeaders))
        {
            var authHeader = authHeaders.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length);

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var userId =
                    jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ??
                    jwt.Claims.FirstOrDefault(c => c.Type == "appid")?.Value ??
                    jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value ??
                    jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var prefix = jwt.Claims.Any(c => c.Type == "oid" || c.Type == "preferred_username")
                        ? "user"
                        : "app";

                    return $"{prefix}:{userId}";
                }
            }
        }

        // IP fallback
        if (headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',')[0]?.Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return $"ip:{ip}";
            }
        }

        return "unknown";
    }
    catch
    {
        return "unknown";
    }
}

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication((context, webApp) =>
    {
        var configuration = context.Configuration;

        webApp.UseThrottlingTroll(options =>
        {
            options.Config = new ThrottlingTrollConfig
            {
                Rules = new[]
                {
                    new ThrottlingTrollRule
                    {
                        LimitMethod = new SlidingWindowRateLimitMethod
                        {
                            PermitLimit = int.Parse(configuration["RateLimiting:PermitLimit"] ?? "100"),
                            IntervalInSeconds = int.Parse(configuration["RateLimiting:IntervalInSeconds"] ?? "60")
                        },
                        
                        // Exclude the file transfer status endpoint from rate limiting as it is polled frequently
                        UriPattern = "^(?!.*/v1/filetransfer/.*/status).*$",

                        IdentityIdExtractor = request => ExtractIdentityFromRequest(request.Headers),

                        ResponseFabric = async (checkResults, requestProxy, responseProxy, requestAborted) =>
                        {
                            var limitExceededResult = checkResults
                                .OrderByDescending(r => r.RetryAfterInSeconds)
                                .FirstOrDefault(r => r.RequestsRemaining < 0);

                            if (limitExceededResult == null) return;

                            var identity = ExtractIdentityFromRequest(requestProxy.Headers);

                            logger.LogWarning(
                                "Rate limit exceeded for {Identity}. RetryAfter: {RetryAfter}s. Path: {Path}",
                                identity,
                                limitExceededResult.RetryAfterInSeconds,
                                requestProxy.UriWithoutQueryString);

                            responseProxy.StatusCode = StatusCodes.Status429TooManyRequests;
                            responseProxy.SetHttpHeader(HeaderNames.RetryAfter, limitExceededResult.RetryAfterHeaderValue);
                            await responseProxy.WriteAsync("Too many requests. Try again later.");
                        }
                    }
                }
            };
        });

        // note: the order of middleware is important, as it determines the execution flow
        webApp.UseMiddleware<ExceptionHandlingMiddleware>();
        webApp.UseMiddleware<RequestValidationMiddleware>();
    })
    // ✅ Adds ASP.NET Core integration
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration((context, config) =>
    {
        // ✅ Configure Azure Key Vault if KeyVaultUri is provided
        config.AddKeyVaultIfConfigured(config.Build(), logger);
    })
    .ConfigureServices((context, services) =>
    {
        // Get configuration for service registrations
        var configuration = context.Configuration;

        services
            .AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                ConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            })
            .ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            // See: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#managing-log-levels
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            var toRemove = options.Rules
                .FirstOrDefault(rule =>
                    string.Equals(rule.ProviderName, typeof(ApplicationInsightsLoggerProvider).FullName));

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });

        services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();

        services.AddSingleton(provider =>
        {
            return new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://login.microsoftonline.com/{configuration["TenantId"]}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
        });

        services.AddActivityLog();
        services.AddDataClient(configuration);
        services.AddDdeiClient(configuration);
        services.AddDdeiClientTactical();
        services.AddEgressClient(configuration);
        services.AddFileTransferClient(configuration);
        services.AddNetAppClient(configuration);

        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddSingleton<IAppInsightsTelemetryClient, AppInsightsTelemetryClientWrapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
        services.AddSingleton<IInitializationHandler, InitializationHandler>();

        services.AddScoped<ICaseMetadataService, CaseMetadataService>();
        services.AddScoped<ICaseEnrichmentService, CaseEnrichmentService>();
        services.AddScoped<IInitService, InitService>();
        services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();
        services.AddSingleton<IRequestValidator, RequestValidator>();
        services.AddSingleton<ISecurityGroupMetadataService, SecurityGroupMetadataService>();
    })
    .Build();

await host.RunAsync();
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.OpenApi;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Middleware;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using CPS.ComplexCases.NetApp.Extensions;
using CPS.ComplexCases.Common.Handlers;

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(webApp =>
    {
        webApp.UseMiddleware<RequestValidationMiddleware>();
    }) // Adds ASP.NET Core integration
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration((context, config) =>
    {
        // Configure Azure Key Vault if KeyVaultUri is provided
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
            })
            .ConfigureFunctionsApplicationInsights();
        services.AddApplicationInsightsTelemetryProcessor<AzureSdkTraceFilter>();
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

        // Add services with configuration
        services.AddActivityLog();
        services.AddEgressClient(configuration);
        services.AddNetAppClient(configuration);
        services.AddDataClient(configuration);

        // Add telemetry services (required by ActivityLogService)
        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddSingleton<IAppInsightsTelemetryClient, AppInsightsTelemetryClientWrapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
        services.AddSingleton<IInitializationHandler, InitializationHandler>();

        services.AddScoped<ICaseMetadataService, CaseMetadataService>();

        services.Configure<SizeConfig>(
            configuration.GetSection("FileTransfer:SizeConfig"));

        services.Configure<PurgeRetentionConfig>(
            configuration.GetSection(PurgeRetentionConfig.SectionName));

        services.AddScoped<IStorageClientFactory, StorageClientFactory>();
        services.AddScoped<IRequestValidator, RequestValidator>();
        services.AddScoped<ITransferEntityHelper, TransferEntityHelper>();

        services.AddDurableTaskClient(x => { x.UseGrpc(); });
        // Configure OpenAPI
        services.AddSingleton<IOpenApiConfigurationOptions, FileTransferApiOpenApiConfigurationOptions>();
    })
    .Build();

await host.RunAsync();
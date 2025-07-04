using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.OpenApi;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.NetApp.Extensions;
using Microsoft.Extensions.Logging;

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // ✅ Adds ASP.NET Core integration
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
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        // ✅ Add services with configuration
        services.AddActivityLog();
        services.AddEgressClient(configuration);
        services.AddNetAppClient(configuration);
        services.AddDataClient(configuration);

        services.AddScoped<ICaseMetadataService, CaseMetadataService>();

        services.Configure<SizeConfig>(
            configuration.GetSection("FileTransfer:SizeConfig"));

        services.AddScoped<IStorageClientFactory, StorageClientFactory>();
        services.AddScoped<IRequestValidator, RequestValidator>();

        // Configure OpenAPI
        services.AddSingleton<IOpenApiConfigurationOptions, FileTransferApiOpenApiConfigurationOptions>();
    })
    .Build();

await host.RunAsync();
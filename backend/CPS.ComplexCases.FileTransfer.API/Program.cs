using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.OpenApi;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.NetApp.Extensions;
using CPS.ComplexCases.FileTransfer.API.Middleware;

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(webApp =>
    {
        webApp.UseMiddleware<RequestValidationMiddleware>();
    }) // ✅ Adds ASP.NET Core integration
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

        services.AddScoped<IStorageClientFactory, StorageClientFactory>();
        services.AddScoped<IRequestValidator, RequestValidator>();
        services.AddScoped<ITransferEntityHelper, TransferEntityHelper>();

        services.AddDurableTaskClient(x =>
        {
            x.UseGrpc();
        });
        // Configure OpenAPI
        services.AddSingleton<IOpenApiConfigurationOptions, FileTransferApiOpenApiConfigurationOptions>();
    })
    .Build();

await host.RunAsync();
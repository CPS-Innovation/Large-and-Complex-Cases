using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.API.Middleware;
using CPS.ComplexCases.API.OpenApi;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.DDEI.Extensions;
using CPS.ComplexCases.DDEI.Tactical.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.NetApp.Extensions;

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(webApp =>
    { 
        // note: the order of middleware is important, as it determines the execution flow
        webApp.UseMiddleware<ExceptionHandlingMiddleware>();
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

        services.AddScoped<ICaseMetadataService, CaseMetadataService>();
        services.AddScoped<ICaseEnrichmentService, CaseEnrichmentService>();
        services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();
        services.AddSingleton<IRequestValidator, RequestValidator>();
    })
    .Build();

await host.RunAsync();
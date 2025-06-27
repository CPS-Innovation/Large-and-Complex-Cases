using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
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

var builder = FunctionsApplication.CreateBuilder(args);

// Create a temporary logger for configuration phase
using var loggerFactory = LoggerFactory.Create(configure => configure.AddConsole());
var logger = loggerFactory.CreateLogger("Configuration");

// Configure Azure Key Vault if KeyVaultUri is provided
builder.Configuration.AddKeyVaultIfConfigured(builder.Configuration, logger);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddActivityLog();
builder.Services.AddEgressClient(builder.Configuration);
builder.Services.AddNetAppClient(builder.Configuration);
builder.Services.AddDataClient(builder.Configuration);

builder.Services.AddScoped<ICaseMetadataService, CaseMetadataService>();

builder.Services.Configure<SizeConfig>(
    builder.Configuration.GetSection("FileTransfer:SizeConfig"));

builder.Services.AddScoped<IStorageClientFactory, StorageClientFactory>();
builder.Services.AddScoped<IRequestValidator, RequestValidator>();

// Configure OpenAPI
builder.Services.AddSingleton<IOpenApiConfigurationOptions, FileTransferApiOpenApiConfigurationOptions>();

await builder.Build().RunAsync();
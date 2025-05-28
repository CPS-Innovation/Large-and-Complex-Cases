using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.Configure<SizeConfig>(
    builder.Configuration.GetSection("FileTransfer:SizeConfig"));

builder.Services.AddScoped<EgressStorageClient>();
builder.Services.AddScoped<IStorageClientFactory, StorageClientFactory>();

builder.Build().Run();

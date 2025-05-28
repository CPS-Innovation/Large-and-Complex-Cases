using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.NetApp.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddEgressClient(builder.Configuration);
builder.Services.AddNetAppClient(builder.Configuration);

builder.Services.Configure<SizeConfig>(
    builder.Configuration.GetSection("FileTransfer:SizeConfig"));

builder.Services.AddScoped<IStorageClientFactory, StorageClientFactory>();

builder.Build().Run();
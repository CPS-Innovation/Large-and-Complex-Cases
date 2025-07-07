using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.NetApp.Extensions;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

var builder = FunctionsApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ITransferEntityReader, TransferEntityReader>();

await builder.Build().RunAsync();
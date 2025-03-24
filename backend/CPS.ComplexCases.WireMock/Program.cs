using CPS.ComplexCases.WireMock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WireMock.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging => logging.AddConsole().AddDebug());

builder.Services.Configure<WireMockServerSettings>(
    builder.Configuration.GetSection("WireMockServerSettings"));

builder.Services.AddHostedService<HostedService>();

var host = builder.Build();
host.Run();
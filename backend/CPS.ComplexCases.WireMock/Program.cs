using CPS.ComplexCases.NetApp.WireMock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WireMock.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging => logging.AddConsole().AddDebug());

builder.Services.AddSingleton<NetAppWireMock>();

#if DEBUG
builder.Services.Configure<WireMockServerSettings>(
    builder.Configuration.GetSection("WireMockServerSettingsLocal"));
#else
builder.Services.Configure<WireMockServerSettings>(
    builder.Configuration.GetSection("WireMockServerSettings"));
#endif

builder.Services.AddHostedService<WireMockHostedService>();

var host = builder.Build();
host.Run();
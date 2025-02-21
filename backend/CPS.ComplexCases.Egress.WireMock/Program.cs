using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WireMock.Settings;
using CPS.ComplexCases.Egress.WireMock;
using CPS.ComplexCases.Egress.WireMock.Mappings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging => logging.AddConsole().AddDebug());

builder.Services.AddSingleton<IEgressWireMock, EgressWireMock>();
#if DEBUG
builder.Services.Configure<WireMockServerSettings>(
    builder.Configuration.GetSection("WireMockServerSettingsLocal"));
#else
builder.Services.Configure<WireMockServerSettings>(
    builder.Configuration.GetSection("WireMockServerSettings"));
#endif
builder.Services.AddHostedService<WireMockHostedService>();

builder.Services.AddSingleton<IWireMockMapping, WorkspaceTokenMapping>();
builder.Services.AddSingleton<IWireMockMapping, WorkspacePermissionsMapping>();
builder.Services.AddSingleton<IWireMockMapping, CaseMaterialMapping>();
builder.Services.AddSingleton<IWireMockMapping, FindWorkspaceMapping>();
builder.Services.AddSingleton<IWireMockMapping, CaseDocumentMapping>();

var host = builder.Build();
host.Run();
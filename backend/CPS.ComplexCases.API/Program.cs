using Amazon.S3;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.API.Validators;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IUnhandledExceptionHandler, UnhandledExceptionHandler>();

builder.Services.AddTransient<IInitializationHandler, InitializationHandler>();
builder.Services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();

builder.Services.AddTransient<IEgressRequestFactory, EgressRequestFactory>();
builder.Services.AddTransient<IEgressArgFactory, EgressArgFactory>();
builder.Services.Configure<EgressOptions>(builder.Configuration.GetSection("EgressOptions"));
builder.Services.AddHttpClient<IEgressClient, EgressClient>(client =>
{
  var egressServiceUrl = builder.Configuration["EgressOptions:Url"];
  if (string.IsNullOrEmpty(egressServiceUrl))
  {
    throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
  }
  client.BaseAddress = new Uri(egressServiceUrl);
});

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<INetAppClient, NetAppClient>();
builder.Services.AddTransient<INetAppArgFactory, NetAppArgFactory>();
builder.Services.AddSingleton(_ =>
{
  // as per https://github.com/dotnet/aspnetcore/issues/43220, there is guidance to only have one instance of ConfigurationManager.
  return new ConfigurationManager<OpenIdConnectConfiguration>(
              $"https://sts.windows.net/{builder.Configuration["TenantId"]}/.well-known/openid-configuration",
              new OpenIdConnectConfigurationRetriever(),
              new HttpDocumentRetriever());
});

builder.Build().Run();

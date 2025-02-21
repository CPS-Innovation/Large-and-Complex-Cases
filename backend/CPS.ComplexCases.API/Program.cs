using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Factories;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

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

builder.Build().Run();

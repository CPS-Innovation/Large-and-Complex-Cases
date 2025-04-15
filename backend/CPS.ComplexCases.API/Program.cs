using CPS.ComplexCases.API.Middleware;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.Data.Extensions;
using CPS.ComplexCases.DDEI.Extensions;
using CPS.ComplexCases.DDEI.Tactical.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.NetApp.Extensions;
using CPS.ComplexCases.OpenApi;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder
    //.UseMiddleware<RequestValidationMiddleware>()
    .UseMiddleware<ExceptionHandlingMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();

builder.Services.AddSingleton(_ =>
{
  // as per https://github.com/dotnet/aspnetcore/issues/43220, there is guidance to only have one instance of ConfigurationManager.
  return new ConfigurationManager<OpenIdConnectConfiguration>(
              $"https://login.microsoftonline.com/{builder.Configuration["TenantId"]}/v2.0/.well-known/openid-configuration",
              new OpenIdConnectConfigurationRetriever(),
              new HttpDocumentRetriever());
});

builder.Services.AddDdeiClient(builder.Configuration);
builder.Services.AddDdeiClientTactical();
builder.Services.AddEgressClient(builder.Configuration);
builder.Services.AddNetAppClient(builder.Configuration);
builder.Services.AddDataClient(builder.Configuration);

builder.Services.AddScoped<ICaseEnrichmentService, CaseEnrichmentService>();
builder.Services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();

builder.Build().Run();

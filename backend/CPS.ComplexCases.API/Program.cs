using Amazon.S3;
using CPS.ComplexCases.API.Middleware;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.DDEI.Extensions;
using CPS.ComplexCases.DDEI.Tactical.Extensions;
using CPS.ComplexCases.Egress.Extensions;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder
    .UseMiddleware<RequestValidationMiddleware>()
    .UseMiddleware<ExceptionHandlingMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();

builder.Services.AddDdeiClient(builder.Configuration);
builder.Services.AddDdeiClientTactical(builder.Configuration);

builder.Services.AddEgressClient(builder.Configuration);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
//builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<INetAppClient, NetAppClient>();
builder.Services.AddTransient<INetAppArgFactory, NetAppArgFactory>();
builder.Services.AddSingleton<IAmazonS3UtilsWrapper, AmazonS3UtilsWrapper>();
builder.Services.Configure<NetAppOptions>(builder.Configuration.GetSection("NetAppOptions"));
builder.Services.AddTransient<IAmazonS3, AmazonS3Client>(client =>
{
  var s3ClientConfig = new AmazonS3Config
  {
    ServiceURL = builder.Configuration["NetAppOptions:Url"],
    ForcePathStyle = true,
    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["NetAppOptions:Region"])
  };

  var credentials = new Amazon.Runtime.BasicAWSCredentials(builder.Configuration["NetAppOptions:AccessKey"], builder.Configuration["NetAppOptions:SecretKey"]);
  return new AmazonS3Client(credentials, s3ClientConfig);
});
builder.Services.AddSingleton(_ =>
{
  // as per https://github.com/dotnet/aspnetcore/issues/43220, there is guidance to only have one instance of ConfigurationManager.
  return new ConfigurationManager<OpenIdConnectConfiguration>(
              $"https://sts.windows.net/{builder.Configuration["TenantId"]}/.well-known/openid-configuration",
              new OpenIdConnectConfigurationRetriever(),
              new HttpDocumentRetriever());
});

builder.Build().Run();

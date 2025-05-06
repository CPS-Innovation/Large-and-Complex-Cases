using Amazon.S3;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.NetApp.Extensions;

public static class IServiceCollectionExtension
{
  public static void AddNetAppClient(this IServiceCollection services, IConfiguration configuration)
  {

    services.AddDefaultAWSOptions(configuration.GetAWSOptions());
    //services.AddAWSService<IAmazonS3>();
    services.Configure<NetAppOptions>(configuration.GetSection("NetAppOptions"));

    var enableMock = configuration.GetValue<bool>("NetAppOptions:EnableMock");

    services.AddTransient<INetAppArgFactory, NetAppArgFactory>();
    if (enableMock)
    {
      services.AddSingleton<INetAppMockHttpRequestFactory, NetAppMockHttpRequestFactory>();
      services.AddHttpClient<INetAppClient, NetAppMockHttpClient>(client =>
    {
      var netAppServiceUrl = configuration["NetAppOptions:Url"];
      if (string.IsNullOrEmpty(netAppServiceUrl))
      {
        throw new ArgumentNullException(nameof(netAppServiceUrl), "NetAppOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(netAppServiceUrl);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    }
    else
    {
      services.AddTransient<INetAppClient, NetAppClient>();
      services.AddSingleton<IAmazonS3UtilsWrapper, AmazonS3UtilsWrapper>();

      services.AddTransient<IAmazonS3, AmazonS3Client>(client =>
      {
        var s3ClientConfig = new AmazonS3Config
        {
          ServiceURL = configuration["NetAppOptions:Url"],
          ForcePathStyle = true,
          RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["NetAppOptions:RegionName"])
        };

        var credentials = new Amazon.Runtime.BasicAWSCredentials(configuration["NetAppOptions:AccessKey"], configuration["NetAppOptions:SecretKey"]);
        return new AmazonS3Client(credentials, s3ClientConfig);
      });
    }
  }
}
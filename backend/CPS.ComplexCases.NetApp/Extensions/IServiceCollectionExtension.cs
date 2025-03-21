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
    services.AddTransient<INetAppClient, NetAppClient>();
    services.AddTransient<INetAppArgFactory, NetAppArgFactory>();
    services.AddSingleton<IAmazonS3UtilsWrapper, AmazonS3UtilsWrapper>();
    services.Configure<NetAppOptions>(configuration.GetSection("NetAppOptions"));
    services.AddTransient<IAmazonS3, AmazonS3Client>(client =>
    {
      var s3ClientConfig = new AmazonS3Config
      {
        ServiceURL = configuration["NetAppOptions:Url"],
        ForcePathStyle = true,
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["NetAppOptions:Region"])
      };

      var credentials = new Amazon.Runtime.BasicAWSCredentials(configuration["NetAppOptions:AccessKey"], configuration["NetAppOptions:SecretKey"]);
      return new AmazonS3Client(credentials, s3ClientConfig);
    });
  }
}
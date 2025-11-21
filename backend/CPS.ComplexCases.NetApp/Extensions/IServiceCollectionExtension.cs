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
		services.Configure<NetAppOptions>(configuration.GetSection("NetAppOptions"));
		services.AddTransient<INetAppArgFactory, NetAppArgFactory>();

		var enableMock = configuration.GetValue<bool>("NetAppOptions:EnableMock");

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
			services.AddTransient<INetAppRequestFactory, NetAppRequestFactory>();
			services.AddTransient<INetAppClient, NetAppClient>();
			services.AddSingleton<IAmazonS3UtilsWrapper, AmazonS3UtilsWrapper>();
			services.AddScoped<IS3ClientFactory, S3ClientFactory>();

			services.AddHttpClient<INetAppHttpClient, NetAppHttpClient>(client =>
			{
				var netAppServiceUrl = configuration["NetAppOptions:ClusterUrl"];
				if (string.IsNullOrEmpty(netAppServiceUrl))
				{
					throw new ArgumentNullException(nameof(netAppServiceUrl), "NetAppOptions:ClusterUrl configuration is missing or empty.");
				}
				client.BaseAddress = new Uri(netAppServiceUrl);

			})
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
			})
			.SetHandlerLifetime(TimeSpan.FromMinutes(5));
		}
		services.AddTransient<NetAppStorageClient>();
	}
}
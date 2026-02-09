using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Services;
using CPS.ComplexCases.NetApp.Telemetry;
using CPS.ComplexCases.NetApp.Wrappers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Wrap;

namespace CPS.ComplexCases.NetApp.Extensions;

public static class IServiceCollectionExtension
{
	private const int RetryAttempts = 3;
	private const int FirstRetryDelaySeconds = 1;

	public static void AddNetAppClient(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDefaultAWSOptions(configuration.GetAWSOptions());
		services.Configure<NetAppOptions>(configuration.GetSection("NetAppOptions"));
		services.AddTransient<INetAppArgFactory, NetAppArgFactory>();
		services.AddTransient<INetAppS3HttpArgFactory, NetAppS3HttpArgFactory>();
		services.AddTransient<INetAppRequestFactory, NetAppRequestFactory>();
		services.AddTransient<INetAppClient, NetAppClient>();
		services.AddSingleton<IAmazonS3UtilsWrapper, AmazonS3UtilsWrapper>();
		services.AddScoped<IS3ClientFactory, S3ClientFactory>();
		services.AddSingleton<IS3CredentialService, S3CredentialService>();
		services.Configure<CryptoOptions>(configuration.GetSection("CryptoOptions"));
		services.AddSingleton<ICryptographyService, CryptographyService>();
		services.AddSingleton<IS3TelemetryHandler, S3TelemetryHandler>();

		services.AddSingleton<IKeyVaultService>(sp =>
		{
			var logger = sp.GetRequiredService<ILogger<KeyVaultService>>();
			var keyVaultUrl = configuration["KeyVault:Url"]
				?? throw new ArgumentNullException("KeyVault:Url", "KeyVault:Url configuration is missing or empty.");

			var secretClient = new SecretClient(
				new Uri(keyVaultUrl),
				new DefaultAzureCredential()
			);

			var sessionDuration = configuration.GetValue<int>("NetAppOptions:SessionDurationSeconds", 3600);
			return new KeyVaultService(secretClient, logger, sessionDuration);
		});

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
		.SetHandlerLifetime(TimeSpan.FromMinutes(5))
		.AddPolicyHandler(GetRetryPolicy());

		services.AddHttpClient<INetAppS3HttpClient, NetAppS3HttpClient>(client =>
		{
			var netAppServiceUrl = configuration["NetAppOptions:Url"];
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
		.SetHandlerLifetime(TimeSpan.FromMinutes(5))
		.AddPolicyHandler(GetRetryPolicy());

		services.AddTransient<NetAppStorageClient>();
	}

	private static AsyncPolicyWrap<HttpResponseMessage> GetRetryPolicy()
	{
		var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
			maxParallelization: 30,
			maxQueuingActions: int.MaxValue
		);

		// https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
		var delay = Backoff.DecorrelatedJitterBackoffV2(
			medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
			retryCount: RetryAttempts);

		static bool responseStatusCodePredicate(HttpResponseMessage response) =>
			response.StatusCode >= HttpStatusCode.InternalServerError
			|| response.StatusCode == HttpStatusCode.TooManyRequests;

		static bool methodPredicate(HttpResponseMessage response) =>
			response.RequestMessage?.Method != HttpMethod.Post
			&& response.RequestMessage?.Method != HttpMethod.Put;

		var retryPolicy = Policy<HttpResponseMessage>
			.HandleResult(r => responseStatusCodePredicate(r) && methodPredicate(r))
			.WaitAndRetryAsync(delay);

		return Policy.WrapAsync(bulkheadPolicy, retryPolicy);
	}
}
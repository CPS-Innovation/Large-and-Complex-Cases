using System.Net;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CPS.ComplexCases.Egress.Extensions;

public static class IServiceCollectionExtension
{
  private const int RetryAttempts = 3;
  private const int FirstRetryDelaySeconds = 1;
  public static void AddEgressClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddTransient<IEgressRequestFactory, EgressRequestFactory>();
    services.AddTransient<IEgressArgFactory, EgressArgFactory>();
    services.Configure<EgressOptions>(configuration.GetSection("EgressOptions"));
    services.AddHttpClient<IEgressClient, EgressClient>(client =>
    {
      var egressServiceUrl = configuration["EgressOptions:Url"];
      if (string.IsNullOrEmpty(egressServiceUrl))
      {
        throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(egressServiceUrl);
      client.Timeout = TimeSpan.FromMinutes(10);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddResilienceHandler("egress-retry", ConfigureResiliencePipeline);

    services.AddHttpClient<EgressStorageClient>(client =>
    {
      var egressServiceUrl = configuration["EgressOptions:Url"];
      if (string.IsNullOrEmpty(egressServiceUrl))
      {
        throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(egressServiceUrl);
      client.Timeout = TimeSpan.FromMinutes(10);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddResilienceHandler("egress-storage-retry", ConfigureResiliencePipeline);
  }

  private static void ConfigureResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> pipeline)
  {
    // Egress API has rate limiting so limit concurrency (equivalent to Polly v7 bulkhead)
    pipeline.AddConcurrencyLimiter(permitLimit: 30, queueLimit: int.MaxValue);

    // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
      MaxRetryAttempts = RetryAttempts,
      Delay = TimeSpan.FromSeconds(FirstRetryDelaySeconds),
      BackoffType = DelayBackoffType.Exponential,
      UseJitter = true,
      ShouldHandle = static args =>
      {
        if (args.Outcome.Result is null)
          return ValueTask.FromResult(false);

        var response = args.Outcome.Result;
        var isRetryableStatus = response.StatusCode >= HttpStatusCode.InternalServerError
            || response.StatusCode == HttpStatusCode.TooManyRequests;
        var isRetryableMethod = response.RequestMessage?.Method != HttpMethod.Post
            && response.RequestMessage?.Method != HttpMethod.Put;

        return ValueTask.FromResult(isRetryableStatus && isRetryableMethod);
      }
    });
  }
}
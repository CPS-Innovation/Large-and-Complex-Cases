using System.Net;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Wrap;

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
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(GetRetryPolicy());

    services.AddHttpClient<EgressStorageClient>(client =>
    {
      var egressServiceUrl = configuration["EgressOptions:Url"];
      if (string.IsNullOrEmpty(egressServiceUrl))
      {
        throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(egressServiceUrl);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(GetRetryPolicy());

  }

  private static AsyncPolicyWrap<HttpResponseMessage> GetRetryPolicy()
  {

    // Egress API has rate limiting policy so we need to respect that and handle 429s
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
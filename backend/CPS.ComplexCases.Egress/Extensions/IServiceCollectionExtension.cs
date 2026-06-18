using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Common.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace CPS.ComplexCases.Egress.Extensions;

public static class IServiceCollectionExtension
{
  private const int RetryAttempts = 3;
  private const int FirstRetryDelaySeconds = 1;

  // Egress rate-limits us with 429s (excluded from the breaker), so a slightly longer sampling window
  // avoids tripping on normal throttling while still catching a genuinely failing service.
  private const double CircuitBreakerFailureThreshold = 0.5;
  private const int CircuitBreakerSamplingDurationSeconds = 60;
  private const int CircuitBreakerMinimumThroughput = 10;
  private const int CircuitBreakerDurationOfBreakSeconds = 30;

  // Created once on first request and reused so circuit-breaker state is shared across all calls per client.
  private static IAsyncPolicy<HttpResponseMessage>? _egressClientPolicy;
  private static IAsyncPolicy<HttpResponseMessage>? _egressStorageClientPolicy;

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
    .AddPolicyHandler((sp, _) => _egressClientPolicy ??= GetResiliencePolicy(sp.GetRequiredService<ILoggerFactory>()));

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
    .AddPolicyHandler((sp, _) => _egressStorageClientPolicy ??= GetResiliencePolicy(sp.GetRequiredService<ILoggerFactory>()));
  }

  internal static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy(ILoggerFactory loggerFactory)
  {
    var logger = loggerFactory.CreateLogger("CPS.ComplexCases.Egress.CircuitBreaker");

    return HttpResiliencePolicyFactory.CreateRateLimitedResiliencePolicy(logger, new HttpResilienceOptions
    {
      ServiceName = "Egress",
      RetryAttempts = RetryAttempts,
      FirstRetryDelay = TimeSpan.FromSeconds(FirstRetryDelaySeconds),
      CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
      CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(CircuitBreakerSamplingDurationSeconds),
      CircuitBreakerMinimumThroughput = CircuitBreakerMinimumThroughput,
      CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(CircuitBreakerDurationOfBreakSeconds),
      BulkheadMaxParallelization = 30,
    });
  }
}
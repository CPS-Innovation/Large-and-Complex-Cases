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

  // Built once per client and reused so circuit-breaker state is shared across all calls for that client.
  private static IServiceProvider? _serviceProvider;
  private static readonly Lazy<IAsyncPolicy<HttpResponseMessage>> _egressClientPolicy =
      new(() => GetResiliencePolicy(_serviceProvider!.GetRequiredService<ILoggerFactory>()));
  private static readonly Lazy<IAsyncPolicy<HttpResponseMessage>> _egressStorageClientPolicy =
      new(() => GetResiliencePolicy(_serviceProvider!.GetRequiredService<ILoggerFactory>()));

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
      client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("EgressOptions:ManagementTimeoutSeconds", 100));
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler((sp, _) =>
    {
      _serviceProvider ??= sp;
      return _egressClientPolicy.Value;
    });

    services.AddHttpClient<EgressStorageClient>(client =>
    {
      var egressServiceUrl = configuration["EgressOptions:Url"];
      if (string.IsNullOrEmpty(egressServiceUrl))
      {
        throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(egressServiceUrl);
      // EgressStorageClient mixes short management calls with file streamed downloads and
      // chunk uploads. The body read of a streamed download is bound by HttpClient.Timeout, so a
      // single short value would break large downloads. Per-operation timeouts are enforced inside
      // BaseEgressClient via CancellationToken instead, leaving the client itself uncapped.
      client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler((sp, _) =>
    {
      _serviceProvider ??= sp;
      return _egressStorageClientPolicy.Value;
    });
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
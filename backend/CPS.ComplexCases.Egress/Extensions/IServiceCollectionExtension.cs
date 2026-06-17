using System.Net;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;

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

  // Retry is kept outermost so a retry attempt re-enters the (possibly open) circuit and fails fast.
  // The circuit breaker sits between retry and the bulkhead so an open circuit short-circuits before
  // consuming a bulkhead slot.
  internal static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy(ILoggerFactory loggerFactory)
  {
    var logger = loggerFactory.CreateLogger("CPS.ComplexCases.Egress.CircuitBreaker");

    // Egress API has rate limiting policy so we need to respect that and handle 429s
    var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
        maxParallelization: 30,
        maxQueuingActions: int.MaxValue
    );

    var circuitBreaker = GetCircuitBreakerPolicy(
        logger,
        CircuitBreakerFailureThreshold,
        TimeSpan.FromSeconds(CircuitBreakerSamplingDurationSeconds),
        CircuitBreakerMinimumThroughput,
        TimeSpan.FromSeconds(CircuitBreakerDurationOfBreakSeconds));

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

    return Policy.WrapAsync(retryPolicy, circuitBreaker, bulkheadPolicy);
  }

  // Only "service is down" signals (5xx and connection failures) trip the breaker. 429s are expected
  // rate limiting, not a service outage, so they are deliberately excluded.
  internal static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
      ILogger logger,
      double failureThreshold,
      TimeSpan samplingDuration,
      int minimumThroughput,
      TimeSpan durationOfBreak)
  {
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
        .AdvancedCircuitBreakerAsync(
            failureThreshold,
            samplingDuration,
            minimumThroughput,
            durationOfBreak,
            onBreak: (outcome, breakDelay) => logger.LogError(
                outcome.Exception,
                "Egress circuit opened for {BreakDelaySeconds}s after status {StatusCode}.",
                breakDelay.TotalSeconds,
                outcome.Result?.StatusCode),
            onReset: () => logger.LogInformation("Egress circuit reset; calls are flowing again."),
            onHalfOpen: () => logger.LogInformation("Egress circuit half-open; testing the next call."));
  }
}
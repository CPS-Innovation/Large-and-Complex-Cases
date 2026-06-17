using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace CPS.ComplexCases.Common.Resilience;

public static class HttpResiliencePolicyFactory
{
  // Retry uses decorrelated jitter backoff to spread out retries and avoid synchronised retry storms.
  public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
      int retryAttempts,
      TimeSpan firstRetryDelay,
      Func<HttpResponseMessage, bool> shouldRetry,
      bool handleHttpRequestException)
  {
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
    var delay = Backoff.DecorrelatedJitterBackoffV2(
        medianFirstRetryDelay: firstRetryDelay,
        retryCount: retryAttempts);

    var builder = handleHttpRequestException
        ? Policy.Handle<HttpRequestException>().OrResult<HttpResponseMessage>(r => shouldRetry(r))
        : Policy<HttpResponseMessage>.HandleResult(r => shouldRetry(r));

    return builder.WaitAndRetryAsync(delay);
  }

  // Only "service is down" signals (5xx and connection failures) trip the breaker. Non-health signals
  // such as 404 or 429 are deliberately excluded even when retry handles them.
  public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
      ILogger logger,
      string serviceName,
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
                "{ServiceName} circuit opened for {BreakDelaySeconds}s after status {StatusCode}.",
                serviceName,
                breakDelay.TotalSeconds,
                outcome.Result?.StatusCode),
            onReset: () => logger.LogInformation("{ServiceName} circuit reset; calls are flowing again.", serviceName),
            onHalfOpen: () => logger.LogInformation("{ServiceName} circuit half-open; testing the next call.", serviceName));
  }

  public static IAsyncPolicy<HttpResponseMessage> CreateBulkheadPolicy(
      int maxParallelization,
      int maxQueuingActions = int.MaxValue)
  {
    return Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization, maxQueuingActions);
  }

  // Retries are only safe for idempotent methods, so POST and PUT are excluded.
  public static bool ExcludesPostAndPut(HttpResponseMessage response) =>
      response.RequestMessage?.Method != HttpMethod.Post
      && response.RequestMessage?.Method != HttpMethod.Put;
}

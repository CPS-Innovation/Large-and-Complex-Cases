using System.Net;
using CPS.ComplexCases.Common.Resilience;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace CPS.ComplexCases.Common.Extensions;

public static class ResiliencePipelineExtensions
{
  // Configures a standard HTTP resilience pipeline built on Microsoft.Extensions.Http.Resilience
  // (Polly v8). Strategies are added outer-to-inner: concurrency limiter, then retry, then circuit
  // breaker. Retry sits outside the breaker so a retry attempt re-enters the (possibly open) circuit
  // and fails fast instead of bypassing it, matching the Microsoft standard resilience ordering.
  public static ResiliencePipelineBuilder<HttpResponseMessage> AddStandardHttpResilience(
      this ResiliencePipelineBuilder<HttpResponseMessage> pipeline,
      HttpResilienceOptions options,
      ILogger logger)
  {
    // Concurrency limiter outermost: shed excess load before doing any work.
    if (options.ConcurrencyLimit > 0)
    {
      pipeline.AddConcurrencyLimiter(permitLimit: options.ConcurrencyLimit, queueLimit: int.MaxValue);
    }

    // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
    if (options.RetryAttempts > 0)
    {
      pipeline.AddRetry(new HttpRetryStrategyOptions
      {
        MaxRetryAttempts = options.RetryAttempts,
        Delay = options.FirstRetryDelay,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = args =>
        {
          // Connection failures are retried regardless of method, since there is no response to
          // inspect. POST/PUT are only excluded on the status-code path below.
          if (options.RetryOnConnectionFailure && args.Outcome.Exception is HttpRequestException)
          {
            return ValueTask.FromResult(true);
          }

          if (args.Outcome.Result is null)
          {
            return ValueTask.FromResult(false);
          }

          var response = args.Outcome.Result;
          var isRetryableStatus = response.StatusCode >= HttpStatusCode.InternalServerError
              || options.AdditionalRetryableStatusCodes.Contains(response.StatusCode);

          return ValueTask.FromResult(isRetryableStatus && ExcludesPostAndPut(response));
        }
      });
    }

    // Only "service is down" signals (5xx and connection failures) trip the breaker. Non-health
    // signals such as 404 or 429 are deliberately excluded even when retry handles them.
    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
      FailureRatio = options.CircuitBreakerFailureThreshold,
      SamplingDuration = options.CircuitBreakerSamplingDuration,
      MinimumThroughput = options.CircuitBreakerMinimumThroughput,
      BreakDuration = options.CircuitBreakerDurationOfBreak,
      ShouldHandle = args =>
      {
        if (args.Outcome.Exception is HttpRequestException)
        {
          return ValueTask.FromResult(true);
        }

        if (args.Outcome.Result is null)
        {
          return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(args.Outcome.Result.StatusCode >= HttpStatusCode.InternalServerError);
      },
      OnOpened = args =>
      {
        logger.LogError(
            args.Outcome.Exception,
            "{ServiceName} circuit opened for {BreakDelaySeconds}s after status {StatusCode}.",
            options.ServiceName,
            args.BreakDuration.TotalSeconds,
            args.Outcome.Result?.StatusCode);
        return default;
      },
      OnClosed = _ =>
      {
        logger.LogInformation("{ServiceName} circuit reset; calls are flowing again.", options.ServiceName);
        return default;
      },
      OnHalfOpened = _ =>
      {
        logger.LogInformation("{ServiceName} circuit half-open; testing the next call.", options.ServiceName);
        return default;
      }
    });

    return pipeline;
  }

  // Retries are only safe for idempotent methods, so POST and PUT are excluded.
  private static bool ExcludesPostAndPut(HttpResponseMessage response) =>
      response.RequestMessage?.Method != HttpMethod.Post
      && response.RequestMessage?.Method != HttpMethod.Put;
}

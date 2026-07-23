using System.Net;

namespace CPS.ComplexCases.Common.Resilience;

public sealed record HttpResilienceOptions
{
  public required string ServiceName { get; init; }
  public required int RetryAttempts { get; init; }
  public required TimeSpan FirstRetryDelay { get; init; }
  public required double CircuitBreakerFailureThreshold { get; init; }
  public required TimeSpan CircuitBreakerSamplingDuration { get; init; }
  public required int CircuitBreakerMinimumThroughput { get; init; }
  public required TimeSpan CircuitBreakerDurationOfBreak { get; init; }

  // When false, the circuit breaker is omitted. Used by clients that only need the shared retry
  // (and optional concurrency) semantics — e.g. FileTransfer — without fail-fast shedding.
  public bool EnableCircuitBreaker { get; init; } = true;

  // Maximum number of concurrent requests allowed through to the service. Set to 0 to disable the
  // concurrency limiter (e.g. for low-volume request/response services).
  public int ConcurrencyLimit { get; init; }

  // Retry connection-level failures (HttpRequestException) in addition to retryable status codes.
  // The rate-limited services rely on status-code retries only, whereas MDS (DDEI) also retries
  // transient connection failures.
  public bool RetryOnConnectionFailure { get; init; }

  // Status codes (in addition to 5xx) that should be retried, e.g. 404 for MDS or 429 for
  // rate-limited services. POST/PUT are always excluded because retries are only safe for
  // idempotent methods.
  public IReadOnlyCollection<HttpStatusCode> AdditionalRetryableStatusCodes { get; init; } = [];
}

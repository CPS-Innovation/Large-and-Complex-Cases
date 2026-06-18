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
  public required int BulkheadMaxParallelization { get; init; }
}

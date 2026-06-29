using System.Net;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.CircuitBreaker;

namespace CPS.ComplexCases.Common.Tests.Extensions;

public class ResiliencePipelineExtensionsTests
{
  private const int MinimumThroughput = 4;

  // Retry is disabled so each call maps to exactly one circuit-breaker sample, isolating breaker behaviour.
  private static ResiliencePipeline<HttpResponseMessage> BuildPipeline(
      TimeSpan? breakDuration = null,
      IReadOnlyCollection<HttpStatusCode>? additionalRetryableStatusCodes = null) =>
      new ResiliencePipelineBuilder<HttpResponseMessage>()
          .AddStandardHttpResilience(new HttpResilienceOptions
          {
            ServiceName = "Test",
            RetryAttempts = 0,
            FirstRetryDelay = TimeSpan.FromMilliseconds(1),
            CircuitBreakerFailureThreshold = 0.5,
            CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(10),
            CircuitBreakerMinimumThroughput = MinimumThroughput,
            // Polly v8 requires a minimum break duration of 500ms.
            CircuitBreakerDurationOfBreak = breakDuration ?? TimeSpan.FromMilliseconds(500),
            ConcurrencyLimit = 0,
            AdditionalRetryableStatusCodes = additionalRetryableStatusCodes ?? [],
          }, NullLogger.Instance)
          .Build();

  private static ValueTask<HttpResponseMessage> Respond(HttpStatusCode statusCode) =>
      ValueTask.FromResult(new HttpResponseMessage(statusCode));

  [Fact]
  public async Task CircuitBreaker_OpensAfterRepeatedServerErrors()
  {
    var pipeline = BuildPipeline();

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.InternalServerError));
    }

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.InternalServerError)).AsTask());
  }

  [Fact]
  public async Task CircuitBreaker_WhenOpen_FailsFastWithoutInvokingDelegate()
  {
    var pipeline = BuildPipeline();

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.InternalServerError));
    }

    var invocations = 0;

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        pipeline.ExecuteAsync(_ =>
        {
          invocations++;
          return Respond(HttpStatusCode.OK);
        }).AsTask());

    Assert.Equal(0, invocations);
  }

  [Fact]
  public async Task CircuitBreaker_ClosesAgainAfterBreakDurationWhenServiceRecovers()
  {
    var pipeline = BuildPipeline(TimeSpan.FromMilliseconds(500));

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.InternalServerError));
    }

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.InternalServerError)).AsTask());

    await Task.Delay(TimeSpan.FromMilliseconds(800));

    var recovered = await pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.OK));
    Assert.Equal(HttpStatusCode.OK, recovered.StatusCode);

    var subsequent = await pipeline.ExecuteAsync(_ => Respond(HttpStatusCode.OK));
    Assert.Equal(HttpStatusCode.OK, subsequent.StatusCode);
  }

  [Theory]
  [InlineData(HttpStatusCode.NotFound)]
  [InlineData(HttpStatusCode.TooManyRequests)]
  public async Task CircuitBreaker_DoesNotOpenForNonServerErrorResponses(HttpStatusCode statusCode)
  {
    var pipeline = BuildPipeline();

    for (var i = 0; i < MinimumThroughput * 2; i++)
    {
      await pipeline.ExecuteAsync(_ => Respond(statusCode));
    }

    var response = await pipeline.ExecuteAsync(_ => Respond(statusCode));
    Assert.Equal(statusCode, response.StatusCode);
  }

  [Fact]
  public async Task Retry_RetriesConfiguredStatusCodesForIdempotentMethods()
  {
    var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddStandardHttpResilience(new HttpResilienceOptions
        {
          ServiceName = "Test",
          RetryAttempts = 2,
          FirstRetryDelay = TimeSpan.FromMilliseconds(1),
          CircuitBreakerFailureThreshold = 0.5,
          CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(30),
          CircuitBreakerMinimumThroughput = 100,
          CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(30),
          ConcurrencyLimit = 0,
          AdditionalRetryableStatusCodes = [HttpStatusCode.TooManyRequests],
        }, NullLogger.Instance)
        .Build();

    var attempts = 0;

    await pipeline.ExecuteAsync(_ =>
    {
      attempts++;
      var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test");
      return ValueTask.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests) { RequestMessage = request });
    });

    Assert.Equal(3, attempts);
  }

  [Fact]
  public async Task Retry_DoesNotRetryNonIdempotentMethods()
  {
    var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddStandardHttpResilience(new HttpResilienceOptions
        {
          ServiceName = "Test",
          RetryAttempts = 2,
          FirstRetryDelay = TimeSpan.FromMilliseconds(1),
          CircuitBreakerFailureThreshold = 0.5,
          CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(30),
          CircuitBreakerMinimumThroughput = 100,
          CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(30),
          ConcurrencyLimit = 0,
          AdditionalRetryableStatusCodes = [HttpStatusCode.TooManyRequests],
        }, NullLogger.Instance)
        .Build();

    var attempts = 0;

    await pipeline.ExecuteAsync(_ =>
    {
      attempts++;
      var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test");
      return ValueTask.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError) { RequestMessage = request });
    });

    Assert.Equal(1, attempts);
  }
}

using System.Net;
using CPS.ComplexCases.DDEI.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.CircuitBreaker;

namespace CPS.ComplexCases.DDEI.Tests.Extensions;

public class DdeiResiliencePolicyTests
{
  private const int MinimumThroughput = 4;

  private static IAsyncPolicy<HttpResponseMessage> CreateBreaker(TimeSpan? durationOfBreak = null) =>
      IServiceCollectionExtension.GetCircuitBreakerPolicy(
          NullLogger.Instance,
          failureThreshold: 0.5,
          samplingDuration: TimeSpan.FromSeconds(10),
          minimumThroughput: MinimumThroughput,
          durationOfBreak: durationOfBreak ?? TimeSpan.FromMilliseconds(300));

  private static Task<HttpResponseMessage> Respond(HttpStatusCode statusCode) =>
      Task.FromResult(new HttpResponseMessage(statusCode));

  [Fact]
  public async Task CircuitBreaker_OpensAfterRepeatedServerErrors()
  {
    var policy = CreateBreaker();

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await policy.ExecuteAsync(() => Respond(HttpStatusCode.InternalServerError));
    }

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        policy.ExecuteAsync(() => Respond(HttpStatusCode.InternalServerError)));
  }

  [Fact]
  public async Task CircuitBreaker_WhenOpen_FailsFastWithoutInvokingDelegate()
  {
    var policy = CreateBreaker();

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await policy.ExecuteAsync(() => Respond(HttpStatusCode.InternalServerError));
    }

    var invocations = 0;

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        policy.ExecuteAsync(() =>
        {
          invocations++;
          return Respond(HttpStatusCode.OK);
        }));

    Assert.Equal(0, invocations);
  }

  [Fact]
  public async Task CircuitBreaker_ClosesAgainAfterBreakDurationWhenServiceRecovers()
  {
    var policy = CreateBreaker(TimeSpan.FromMilliseconds(300));

    for (var i = 0; i < MinimumThroughput; i++)
    {
      await policy.ExecuteAsync(() => Respond(HttpStatusCode.InternalServerError));
    }

    await Assert.ThrowsAnyAsync<BrokenCircuitException>(() =>
        policy.ExecuteAsync(() => Respond(HttpStatusCode.InternalServerError)));

    await Task.Delay(TimeSpan.FromMilliseconds(500));

    var recovered = await policy.ExecuteAsync(() => Respond(HttpStatusCode.OK));
    Assert.Equal(HttpStatusCode.OK, recovered.StatusCode);

    var subsequent = await policy.ExecuteAsync(() => Respond(HttpStatusCode.OK));
    Assert.Equal(HttpStatusCode.OK, subsequent.StatusCode);
  }

  [Fact]
  public async Task CircuitBreaker_DoesNotOpenForNotFoundResponses()
  {
    var policy = CreateBreaker();

    for (var i = 0; i < MinimumThroughput * 2; i++)
    {
      await policy.ExecuteAsync(() => Respond(HttpStatusCode.NotFound));
    }

    var response = await policy.ExecuteAsync(() => Respond(HttpStatusCode.NotFound));
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}

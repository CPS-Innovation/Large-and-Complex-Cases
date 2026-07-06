using CPS.ComplexCases.Common.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace CPS.ComplexCases.Common.Tests.Resilience;

public class SharedResiliencePolicyTests
{
  private static ServiceProvider BuildProvider() =>
      new ServiceCollection().AddLogging().BuildServiceProvider();

  [Fact]
  public void GetPolicy_BuildsPolicyOnceAndReusesIt()
  {
    var builds = 0;
    var shared = new SharedResiliencePolicy(_ =>
    {
      builds++;
      return Policy.NoOpAsync<HttpResponseMessage>();
    });

    using var provider = BuildProvider();

    var first = shared.GetPolicy(provider);
    var second = shared.GetPolicy(provider);

    Assert.Same(first, second);
    Assert.Equal(1, builds);
  }

  [Fact]
  public void GetPolicy_AfterFirstProviderDisposed_DoesNotThrowAndReturnsCachedPolicy()
  {
    var shared = new SharedResiliencePolicy(loggerFactory =>
    {
      // Force the same resolution the real wiring relies on, so the test fails if the provider is read after disposal.
      loggerFactory.CreateLogger("test");
      return Policy.NoOpAsync<HttpResponseMessage>();
    });

    var firstProvider = BuildProvider();
    var cached = shared.GetPolicy(firstProvider);

    firstProvider.Dispose();

    var secondProvider = BuildProvider();
    try
    {
      var afterDisposal = shared.GetPolicy(secondProvider);
      Assert.Same(cached, afterDisposal);
    }
    finally
    {
      secondProvider.Dispose();
    }
  }

  [Fact]
  public void GetPolicy_MultiClientOrdering_SecondClientResolvesAfterFirstScopeDisposed()
  {
    // Simulates two clients in one registration, each with its own holder, as produced by AddResiliencePolicyHandler.
    var clientA = new SharedResiliencePolicy(loggerFactory =>
    {
      loggerFactory.CreateLogger("clientA");
      return Policy.NoOpAsync<HttpResponseMessage>();
    });
    var clientB = new SharedResiliencePolicy(loggerFactory =>
    {
      loggerFactory.CreateLogger("clientB");
      return Policy.NoOpAsync<HttpResponseMessage>();
    });

    // Client A runs first and captures its (then live) provider scope.
    var providerA = BuildProvider();
    var policyA = clientA.GetPolicy(providerA);

    // That handler scope rotates and is disposed before client B ever runs.
    providerA.Dispose();

    // Client B's first call must resolve against its own live provider, not a retained disposed one.
    using var providerB = BuildProvider();
    var exception = Record.Exception(() => clientB.GetPolicy(providerB));

    Assert.Null(exception);
    Assert.NotNull(policyA);
  }
}

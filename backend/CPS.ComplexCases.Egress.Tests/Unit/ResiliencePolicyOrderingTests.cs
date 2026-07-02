using CPS.ComplexCases.Common.Resilience;
using CPS.ComplexCases.Egress.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.Egress.Tests.Unit;

// Reproduces the registration ordering that previously threw ObjectDisposedException:
// EgressClient and EgressStorageClient each get their own policy holder, so the second
// client to run must resolve against its own live provider, not a retained, disposed scope.
public class ResiliencePolicyOrderingTests
{
  private static ServiceProvider BuildProvider() =>
      new ServiceCollection().AddLogging().BuildServiceProvider();

  [Fact]
  public void SecondClientResolvesPolicy_AfterFirstClientScopeDisposed()
  {
    var firstClient = new SharedResiliencePolicy(IServiceCollectionExtension.GetResiliencePolicy);
    var secondClient = new SharedResiliencePolicy(IServiceCollectionExtension.GetResiliencePolicy);

    var firstProvider = BuildProvider();
    var firstPolicy = firstClient.GetPolicy(firstProvider);

    // The first client's handler scope rotates and is disposed before the second client first runs.
    firstProvider.Dispose();

    using var secondProvider = BuildProvider();
    var exception = Record.Exception(() => secondClient.GetPolicy(secondProvider));

    Assert.Null(exception);
    Assert.NotNull(firstPolicy);
  }
}

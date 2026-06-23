using CPS.ComplexCases.Common.Resilience;
using CPS.ComplexCases.NetApp.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.NetApp.Tests.Unit.Extensions;

// Reproduces the registration ordering that previously threw ObjectDisposedException:
// NetAppHttpClient and NetAppS3HttpClient each get their own policy holder, so the second
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace CPS.ComplexCases.Common.Resilience;

public static class ResiliencePolicyHttpClientBuilderExtensions
{
  // Wires a circuit-breaker/resilience policy handler that builds the policy once from the first live
  // service provider and caches it per client, without ever retaining the (scoped) provider.
  public static IHttpClientBuilder AddResiliencePolicyHandler(
      this IHttpClientBuilder builder,
      Func<ILoggerFactory, IAsyncPolicy<HttpResponseMessage>> policyFactory)
  {
    var sharedPolicy = new SharedResiliencePolicy(policyFactory);
    return builder.AddPolicyHandler((sp, _) => sharedPolicy.GetPolicy(sp));
  }
}

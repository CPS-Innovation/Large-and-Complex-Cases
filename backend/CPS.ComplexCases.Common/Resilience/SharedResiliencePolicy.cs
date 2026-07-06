using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace CPS.ComplexCases.Common.Resilience;

// Builds a resilience policy once from the first live IServiceProvider and caches it, so circuit-breaker
// state is shared across all calls for a single client (one breaker per client). The provider is only
// used inside the call to resolve the singleton ILoggerFactory and is never retained, which avoids reading
// a disposed scope after an HttpClient handler rotation.
public sealed class SharedResiliencePolicy
{
  private readonly Func<ILoggerFactory, IAsyncPolicy<HttpResponseMessage>> _policyFactory;
  private readonly object _gate = new();
  // volatile so the fast-path read outside the lock cannot observe a non-null reference
  private volatile IAsyncPolicy<HttpResponseMessage>? _policy;

  public SharedResiliencePolicy(Func<ILoggerFactory, IAsyncPolicy<HttpResponseMessage>> policyFactory)
  {
    _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
  }

  public IAsyncPolicy<HttpResponseMessage> GetPolicy(IServiceProvider serviceProvider)
  {
    if (_policy is not null)
    {
      return _policy;
    }

    lock (_gate)
    {
      _policy ??= _policyFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
    }

    return _policy;
  }
}

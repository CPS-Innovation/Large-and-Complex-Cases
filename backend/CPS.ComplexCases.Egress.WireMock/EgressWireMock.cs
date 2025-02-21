using CPS.ComplexCases.Egress.WireMock.Mappings;
using Microsoft.Extensions.Options;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.Egress.WireMock;

public class EgressWireMock : IEgressWireMock, IAsyncDisposable
{
  private readonly WireMockServer _server;
  private readonly IEnumerable<IWireMockMapping> _mappings;

  public EgressWireMock(
      IOptions<WireMockServerSettings> settings,
      IEnumerable<IWireMockMapping> mappings)
  {
    _server = WireMockServer.Start(settings.Value);
    _mappings = mappings;
  }

  public void ConfigureMappings()
  {
    foreach (var mapping in _mappings)
    {
      mapping.Configure(_server);
    }
  }

  public void Dispose()
  {
    _server.Stop();
    _server.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    _server.Stop();
    if (_server is IAsyncDisposable asyncDisposable)
    {
      await asyncDisposable.DisposeAsync();
    }
    else
    {
      _server.Dispose();
    }
  }
}

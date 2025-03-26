using WireMock.Server;

namespace CPS.ComplexCases.WireMock.Core;

public interface IWireMockMapping
{
  void Configure(WireMockServer server);
}
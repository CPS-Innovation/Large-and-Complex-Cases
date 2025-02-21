using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public interface IWireMockMapping
{
  void Configure(WireMockServer server);
}
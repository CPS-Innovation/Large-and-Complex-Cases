using WireMock.Server;

namespace CPS.ComplexCases.NetApp.WireMock.Mappings;

public interface IWireMockMapping
{
    void Configure(WireMockServer server);
}
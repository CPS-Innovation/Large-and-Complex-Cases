using WireMock.Server;

namespace CPS.ComplexCases.WireMock.Core;

public static class LoadMappingsExtension
{
    public static WireMockServer LoadMappings(this WireMockServer server, params IWireMockMapping[] mappings)
    {
        foreach (var mapping in mappings)
        {
            mapping.Configure(server);
        }

        return server;
    }
}
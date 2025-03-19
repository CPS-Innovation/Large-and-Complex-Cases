using WireMock.Server;

namespace CPS.ComplexCases.WireMock.Core;

public static class MappingLoader
{
    public static void Load(WireMockServer server)
    {
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IWireMockMapping).IsAssignableFrom(p) && !p.IsInterface)
            .Select(p => (IWireMockMapping)Activator.CreateInstance(p))
            .ToList()
            .ForEach(m => m.Configure(server));
    }
}
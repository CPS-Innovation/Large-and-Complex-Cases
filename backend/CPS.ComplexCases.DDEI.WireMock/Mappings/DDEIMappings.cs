
using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.DDEI.WireMock.Mappings;

public class DDEIMappings : IWireMockMapping
{
    public void Configure(WireMockServer server)
    {
        Map(server, "/api/units", "units.json");
        Map(server, "/api/user-data", "user-data.json");
        Map(server, "/api/user-filter-data", "user-filter-data.json");

        Map(server, "/api/urns/45CV2911222/cases", "search-by-urn.json");
        Map(server, "/api/cases/find", "search-by-defendant-name.json", new Dictionary<string, string>
        {
            { "area-code", "1057708" },
            { "defendant-name", "Husband" }
        });

        Map(server, "/api/cases/find", "search-by-operation-name.json", new Dictionary<string, string>
        {
            { "area-code", "1057708" },
            { "operation-name", "ottawa" }
        });

        Map(server, "/api/cases/2149297/summary", "case-2149297.json");
        Map(server, "/api/cases/2149309/summary", "case-2149309.json");
        Map(server, "/api/cases/2149310/summary", "case-2149310.json");
        Map(server, "/api/cases/2156682/summary", "case-2156682.json");
    }

    private static void Map(WireMockServer server, string path, string fileName) => Map(server, path, fileName, new Dictionary<string, string>());

    private static void Map(WireMockServer server, string path, string fileName, IDictionary<string, string> queryParams)
    {
        var request = Request.Create().WithPath(path).UsingGet();
        foreach (var queryParam in queryParams)
        {
            request.WithParam(queryParam.Key, queryParam.Value);
        }

        server.Given(request).RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(GetFileContent(fileName)).WithDelay(750)
        );
    }

    private static byte[] GetFileContent(string fileName) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "files", fileName));

}


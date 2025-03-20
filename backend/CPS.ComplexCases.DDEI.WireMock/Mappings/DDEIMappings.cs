
using System.Globalization;
using System.Text.Json;
using System.Web;
using CPS.ComplexCases.WireMock.Core;
using CsvHelper;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.DDEI.WireMock.Mappings;

public class DDEIMappings : IWireMockMapping
{
    private const int _minDelayMs = 100;
    private const int _maxDelayMs = 250;
    private readonly List<Case> _cases = ReadCases();
    private const string _mockUsername = "mock.user";
    private const string _mockCmsModernToken = "00000000-0000-4000-8000-000000000000";
    private const string _mockCmsCookies = $"{_mockUsername}.cookies";
    private const string _cmsAuthValuesHeaderName = "Cms-Auth-Values";

    public void Configure(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/api/authenticate")
                .UsingPost()
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .WithBody(new FormUrlEncodedMatcher([$"username={_mockUsername}", "password=*"])))
            .RespondWith(Response.Create()
                .WithBodyAsJson(new { Token = _mockCmsModernToken, Cookies = _mockCmsCookies }));

        server
            .Given(Request.Create()
                .WithPath("/api/user-filter-data")
                .UsingGet()
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile(FilePath("user-filter-data.json"))
                .WithRandomDelay(_minDelayMs, _maxDelayMs));

        server
            .Given(Request.Create()
                .WithPath("/api/user-data")
                .UsingGet()
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile(FilePath("user-data.json"))
                .WithRandomDelay(_minDelayMs, _maxDelayMs));

        server
            .Given(Request.Create()
                .WithPath("/api/units")
                .UsingGet()
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyFromFile(FilePath("units.json"))
                .WithRandomDelay(_minDelayMs, _maxDelayMs));

        _cases.ForEach(@case =>
        {
            var urnRoot = @case.urn[..2];

            server
                .Given(Request.Create()
                    .WithPath($"/api/urns/{urnRoot}?????????/cases")
                    .UsingGet()
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.urn.StartsWith(urnRoot))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            server
                .Given(Request.Create()
                    .WithPath($"/api/cases/find")
                    .UsingGet()
                    .WithParam("area-code")
                    .WithParam("operation-name", ignoreCase: true, @case.operation)
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.operation.Equals(@case.operation, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            // If we have two cases with the same lead defendant surname, we will register two mappings
            //  but there is no harm in that
            server
                .Given(Request.Create()
                    .WithPath($"/api/cases/find")
                    .UsingGet()
                    .WithParam("area-code")
                    .WithParam("defendant-name", ignoreCase: true, @case.leadDefendantSurname)
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.leadDefendantSurname.Equals(@case.leadDefendantSurname, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            server
               .Given(Request.Create()
                   .WithPath($"/api/cases/{@case.id}/summary")
                   .UsingGet()
                   .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
               )
               .RespondWith(Response.Create()
                   .WithBodyAsJson(_cases
                       .First(c => c.id == @case.id))
                   .WithRandomDelay(_minDelayMs, _maxDelayMs)
               );
        });
    }

    private static bool IsAuthedOrAHumanCheckingInABrowser(IDictionary<string, string[]> headers)
    {
        try
        {
            if (headers.ContainsKey("User-Agent"))
            {
                // If the request has a User-Agent header, it's a human checking in a browser
                return true;
            }
            if (!headers.TryGetValue("Cookie", out string[]? value))
            {
                return false;
            }
            var cookies = value.SelectMany(c => c.Split(';')).Select(c => c.Trim());
            var cmsAuthCookie = cookies.FirstOrDefault(c => c.StartsWith(_cmsAuthValuesHeaderName));
            if (cmsAuthCookie is null)
            {
                return false;
            }
            var cmsAuthValuesString = cmsAuthCookie.Split('=').Last();
            var decodedJsonString = HttpUtility.UrlDecode(cmsAuthValuesString);
            var cmsAuthValues = JsonSerializer.Deserialize<CmsAuthValues>(decodedJsonString);
            return cmsAuthValues?.Token == _mockCmsModernToken && cmsAuthValues.Cookies == _mockCmsCookies;
        }
        catch (Exception)
        {
            return false;
        }
    }


    private static string FilePath(string fileName) => Path.Combine(AppContext.BaseDirectory, "files", fileName);

    private static List<Case> ReadCases()
    {
        using var reader = new StreamReader(FilePath("cases.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return [.. csv.GetRecords<Case>()];
    }

    private class Case
    {
        public int id { get; set; }

        public required string urn { get; set; }

        public required string leadDefendantSurname { get; set; }

        public required string leadDefendantFirstNames { get; set; }

        public required string operation { get; set; }

        public required string registrationDate { get; set; }
    }

    private class CmsAuthValues
    {
        public required string Cookies { get; set; }
        public required string Token { get; set; }
    }
}


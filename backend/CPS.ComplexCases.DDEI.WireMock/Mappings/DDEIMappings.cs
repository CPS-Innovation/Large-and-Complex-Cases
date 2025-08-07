
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
    private const int _minDelayMs = 250;
    private const int _maxDelayMs = 500;
    private readonly List<Case> _cases = ReadCases();
    private const string _mockUsername = "mock.user";
    private const string _mockCmsModernToken = "00000000-0000-4000-8000-000000000000";
    private const string _mockCmsCookies = $"{_mockUsername}.cookies";
    private const string _cmsAuthValuesHeaderName = "Cms-Auth-Values";

    private const int _dominantPriority = 1;
    private const int _fallbackPriority = 2;

    public void Configure(WireMockServer server)
    {
        // Simulate hitting DDEI to obtain CMS authentication via direct login
        server
            .Given(Request.Create()
                .WithPath("/api/authenticate")
                .UsingPost()
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .WithBody(new FormUrlEncodedMatcher([$"username={_mockUsername}", "password=*"])))
            .RespondWith(Response.Create()
                .WithBodyAsJson(new { Token = _mockCmsModernToken, Cookies = _mockCmsCookies }));

        // Lookups
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

        // Lookups
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

        // Lookups
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

        // URN search fallback (see AtPriority(...))
        server
            .Given(Request.Create()
                // URNs are 11 characters long, so we need 9 more question marks
                .WithPath($"/api/urns/???????????/cases")
                .UsingGet()
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
            )
            .AtPriority(_fallbackPriority)
            .RespondWith(Response.Create()
                .WithBodyAsJson(Enumerable.Empty<bool>())
                .WithRandomDelay(_minDelayMs, _maxDelayMs)
            );

        // Operation search fallback (see AtPriority(...))
        server
            .Given(Request.Create()
                .WithPath($"/api/cases/find")
                .UsingGet()
                .WithParam("area-code")
                .WithParam("operation-name")
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
            )
            .AtPriority(_fallbackPriority)
            .RespondWith(Response.Create()
                .WithBodyAsJson(Enumerable.Empty<bool>())
                .WithRandomDelay(_minDelayMs, _maxDelayMs)
            );

        // Defendant search fallback (see AtPriority(...))
        server
            .Given(Request.Create()
                .WithPath($"/api/cases/find")
                .UsingGet()
                .WithParam("area-code")
                .WithParam("defendant-name")
                .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
            )
            .AtPriority(_fallbackPriority)
            .RespondWith(Response.Create()
                .WithBodyAsJson(Enumerable.Empty<bool>())
                .WithRandomDelay(_minDelayMs, _maxDelayMs)
            );

        // For each case record in our csv file...
        _cases.ForEach(@case =>
        {
            var urnRoot = @case.urn[..2];

            // URN search
            server
                .Given(Request.Create()
                    // URNs are 11 characters long, so we need 9 more question marks
                    .WithPath($"/api/urns/{urnRoot}?????????/cases")
                    .UsingGet()
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .AtPriority(_dominantPriority)
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.urn.StartsWith(urnRoot))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            // Operation search
            server
                .Given(Request.Create()
                    .WithPath($"/api/cases/find")
                    .UsingGet()
                    .WithParam("area-code")
                    .WithParam("operation-name", ignoreCase: true, @case.operation)
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .AtPriority(_dominantPriority)
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.operation.Equals(@case.operation, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            // Defendant search: if we have two cases with the same lead defendant last name, we will register two mappings
            //  but there is no harm in that
            server
                .Given(Request.Create()
                    .WithPath($"/api/cases/find")
                    .UsingGet()
                    .WithParam("area-code")
                    .WithParam("defendant-name", ignoreCase: true, @case.leadDefendantSurname)
                    .WithHeader(IsAuthedOrAHumanCheckingInABrowser)
                )
                .AtPriority(_dominantPriority)
                .RespondWith(Response.Create()
                    .WithBodyAsJson(_cases
                        .Where(c => c.leadDefendantSurname.Equals(@case.leadDefendantSurname, StringComparison.OrdinalIgnoreCase))
                        .Select(c => new { c.id, c.urn })
                        .ToList())
                    .WithRandomDelay(_minDelayMs, _maxDelayMs)
                );

            // Getting the case record itself
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
        // Lets allow a human to peruse the mock, but if we are the DDEClient then check that the
        //  CMS auth values are correct.
        try
        {
            if (headers.ContainsKey("User-Agent"))
            {
                // If the request has a User-Agent header, it's a human checking in a browser
                //  and we'll allow the mock to serve a response ...
                return true;
            }
            // ...otherwise, lets make our mock check for correct CMS auth values
            if (!headers.TryGetValue(_cmsAuthValuesHeaderName, out string[]? value))
            {
                return false;
            }

            var decodedJsonString = HttpUtility.UrlDecode(value.First());
            var cmsAuthValues = JsonSerializer.Deserialize<CmsAuthValues>(decodedJsonString);
            return cmsAuthValues?.Token == _mockCmsModernToken
                && cmsAuthValues.Cookies == _mockCmsCookies;
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


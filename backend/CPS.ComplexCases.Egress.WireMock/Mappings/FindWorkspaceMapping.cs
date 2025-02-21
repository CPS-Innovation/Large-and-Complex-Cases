using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class FindWorkspaceMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    server
        .Given(Request.Create()
            .WithPath("/api/v1/workspaces")
            .UsingGet()
            .WithParam("name", "test-workspace"))
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new
            {
              data = new[]
                {
                        new
                        {
                            id = "workspace-id",
                            name = "test-workspace",
                            _links = new { properties = "http://egress.wiremock.com/w/edit/workspace-id" }
                        }
                },
              pagination = new
              {
                current_page_num = 1,
                per_page = 10,
                total_pages = 1,
                total_results = 1
              }
            }));
  }
}
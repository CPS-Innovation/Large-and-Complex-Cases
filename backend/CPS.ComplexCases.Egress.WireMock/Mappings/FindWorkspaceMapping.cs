using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class FindWorkspaceMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    ConfigureFindByOperationName(server);
    ConfigureFind(server);
  }

  private static void ConfigureFindByOperationName(WireMockServer server)
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

  private static void ConfigureFind(WireMockServer server)
  {
    server
  .Given(Request.Create()
      .WithPath("/api/v1/workspaces")
      .UsingGet())
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
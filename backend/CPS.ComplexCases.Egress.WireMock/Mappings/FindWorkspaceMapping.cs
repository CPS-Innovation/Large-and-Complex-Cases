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
        .WithParam("name", "test-workspace")
        .WithParam("skip", "0")
        .WithParam("limit", "100")
        .WithParam("view", "full"))
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
                date_created = "2021-01-01T00:00:00Z",
              }
            },
          data_info = new
          {
            num_returned = 1,
            skip = 0,
            limit = 100,
            total_results = 1
          }
        }));
  }

  private static void ConfigureFind(WireMockServer server)
  {
    server
  .Given(Request.Create()
      .WithPath("/api/v1/workspaces")
      .WithParam("skip", "0")
      .WithParam("limit", "100")
      .WithParam("view", "full")
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
                date_created = "2021-01-01T00:00:00Z",
              }
          },
        data_info = new
        {
          num_returned = 1,
          skip = 0,
          limit = 100,
          total_results = 1
        }
      }));
  }

}
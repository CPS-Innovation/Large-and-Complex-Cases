using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class WorkspacePermissionsMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    server
        .Given(Request.Create()
            .WithPath("/api/v1/workspaces/workspace-id/users")
            .UsingGet()
            )
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new
            {
              data = new[]
                {
                  new
                  {
                    switch_id = "integration@cps.gov.uk",
                    role_id = "582a00a0510c5679d41c085c"
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
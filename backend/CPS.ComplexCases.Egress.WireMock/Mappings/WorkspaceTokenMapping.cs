using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class WorkspaceTokenMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    server
        .Given(Request.Create().WithPath("/api/v1/user/auth/").UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new { token = "mocked-token" }));
  }
}
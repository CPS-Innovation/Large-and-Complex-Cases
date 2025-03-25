using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class CaseDocumentMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    var filePath = Path.Combine(AppContext.BaseDirectory, "files", "example.pdf");
    var fileContent = File.ReadAllBytes(filePath);

    server
        .Given(Request.Create()
            .WithPath("/api/v1/workspaces/workspace-id/files/file-id")
            .UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/pdf")
            .WithHeader("Content-Disposition", "attachment; filename=\"example.pdf\"")
            .WithBody(fileContent));
  }
}
using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class CaseMaterialMapping : IWireMockMapping
{
  public void Configure(WireMockServer server)
  {
    ConfigureRootFilesListing(server);
    ConfigureFolderFilesListing(server);
  }

  private static void ConfigureRootFilesListing(WireMockServer server)
  {
    server
        .Given(Request.Create()
            .WithPath("/api/v1/workspaces/workspace-id/files")
            .UsingGet()
            .WithParam("view", "full")
            .WithParam("skip", "0")
            .WithParam("limit", "10"))
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new
            {
              data = new[]
            {
                  new
                  {
                    id = "file-id",
                    filename = "file-name",
                    path = "file-path",
                    date_updated = "2022-01-01T00:00:00Z",
                    is_folder = false,
                    version = 1
                  },
                  new
                  {
                    id = "folder-id",
                    filename = "folder-name",
                    path = "folder-path",
                    date_updated = "2022-01-04T00:00:00Z",
                    is_folder = true,
                    version = 1
                  }
            },
              data_info = new
              {
                num_returned = 2,
                skip = 0,
                limit = 10,
                total_results = 2
              }
            }));
  }

  private static void ConfigureFolderFilesListing(WireMockServer server)
  {
    server
        .Given(Request.Create()
            .WithPath("/api/v1/workspaces/workspace-id/files")
            .UsingGet()
            .WithParam("view", "full")
            .WithParam("folder", "folder-id")
            .WithParam("skip", "0")
            .WithParam("limit", "10"))
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new
            {
              data = new[]
                {
                  new
                  {
                    id = "nested-file-id",
                    filename = "nested-file-name",
                    path = "folder-id/file-path",
                    date_updated = "2022-01-01T00:00:00Z",
                    is_folder = false,
                    version = 1
                  },
                },
              data_info = new
              {
                num_returned = 1,
                skip = 0,
                limit = 10,
                total_results = 1
              }
            }));
  }

}
using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class CaseMaterialMapping : IWireMockMapping
{
  private const string WorkspaceFilesPath = "/api/v1/workspaces/workspace-id/files";
  private const string DefaultDateUpdated = "2022-01-01T00:00:00Z";

  public void Configure(WireMockServer server)
  {
    ConfigureRootFilesListing(server);
    ConfigureFolderFilesListing(server);
    ConfigurePathFilesListing(server);
    ConfigureFileExistsScenario(server);
  }

  private static void ConfigureRootFilesListing(WireMockServer server)
  {
    RespondWithJson(server,
        FilesRequest()
            .WithParam("skip", "0")
            .WithParam("limit", "10"),
        ListBody(
            FileEntry("file-id", "file-name", "file-path", isFolder: false),
            FileEntry("folder-id", "folder-name", "folder-path", isFolder: true, dateUpdated: "2022-01-04T00:00:00Z")));
  }

  private static void ConfigureFolderFilesListing(WireMockServer server)
  {
    var body = ListBody(FileEntry("nested-file-id", "nested-file-name", "folder-id/file-path", isFolder: false));

    RespondWithJson(server,
        FilesRequest().WithParam("folder", "folder-id"),
        body);

    RespondWithJson(server,
        FilesRequest()
            .WithParam("folder", "folder-id")
            .WithParam("skip", "0")
            .WithParam("limit", "10"),
        body);
  }

  private static void ConfigurePathFilesListing(WireMockServer server)
  {
    RespondWithJson(server,
        FilesRequest().WithParam("path", "folder-path"),
        ListBody(FileEntry("nested-file-id", "nested-file-name", "folder-path/file-path", isFolder: false)));

    // A path that does not exist returns an empty data set with total_results = 0,
    // which the UI can distinguish from a populated folder.
    RespondWithJson(server,
        FilesRequest().WithParam("path", "non-existent-path"),
        ListBody());
  }

  private static void ConfigureFileExistsScenario(WireMockServer server)
  {
    RespondWithJson(server,
        FilesRequest("/api/v1/workspaces/workspace-file-exists/files").WithParam("path", "/uploads/test"),
        ListBody(FileEntry("existing-file-id", "test-file.txt", "/uploads/test/test-file.txt", isFolder: false)));

    RespondWithJson(server,
        FilesRequest().WithParam("path", "/uploads/test"),
        ListBody());
  }

  private static IRequestBuilder FilesRequest(string path = WorkspaceFilesPath) =>
      Request.Create()
          .WithPath(path)
          .UsingGet()
          .WithParam("view", "full");

  private static object FileEntry(string id, string filename, string path, bool isFolder, string dateUpdated = DefaultDateUpdated) => new
  {
    id,
    filename,
    path,
    date_updated = dateUpdated,
    is_folder = isFolder,
    version = 1
  };

  private static object ListBody(params object[] data) => new
  {
    data,
    data_info = new
    {
      num_returned = data.Length,
      skip = 0,
      limit = 10,
      total_results = data.Length
    }
  };

  private static void RespondWithJson(WireMockServer server, IRequestBuilder request, object body) =>
      server
          .Given(request)
          .RespondWith(Response.Create()
              .WithStatusCode(200)
              .WithBodyAsJson(body));
}

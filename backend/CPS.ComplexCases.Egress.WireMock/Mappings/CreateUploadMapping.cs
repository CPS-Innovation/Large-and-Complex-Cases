using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.Egress.WireMock.Mappings;

public class CreateUploadMapping : IWireMockMapping
{
    public void Configure(WireMockServer server)
    {
        ConfigureUploadRequest(server);
        ConfigureUploadChunkRequest(server);
        ConfigureCompleteUploadRequest(server);
    }

    private static void ConfigureUploadRequest(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/api/v1/workspaces/*/uploads")
                .UsingPost()
                .WithHeader("Authorization", "*")
                .WithHeader("Content-Type", "application/json*")
                .WithBody(b =>
                    b != null &&
                    b.Contains("filename") &&
                    b.Contains("filesize") &&
                    b.Contains("folder_path")))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    id = "mock-upload-id-12345",
                    md5_hash = "d41d8cd98f00b204e9800998ecf8427e"
                }));
    }

    private static void ConfigureUploadChunkRequest(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/api/v1/workspaces/*/uploads/*/")
                .UsingPatch()
                .WithHeader("Authorization", "*")
                .WithHeader("Content-Type", "multipart/form-data*"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    chunk_number = 1,
                    content_range = 1024
                }));
    }

    private static void ConfigureCompleteUploadRequest(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/api/v1/workspaces/*/uploads/*/")
                .UsingPut()
                .WithHeader("Authorization", "*")
                .WithHeader("Content-Type", "application/json*")
                .WithBody(b =>
                    b != null &&
                    b.Contains("md5_hash") &&
                    b.Contains("done")))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    id = "mock-upload-id-12345",
                }));
    }
}
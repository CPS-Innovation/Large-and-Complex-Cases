using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.WireMock.Mappings;

public class UploadMapping : IWireMockMapping
{
    public void Configure(WireMockServer server)
    {
        ConfigureUploadRequest(server);
        ConfigureUploadPartRequest(server);
        ConfigureCompleteUploadRequest(server);
    }

    private static void ConfigureUploadRequest(WireMockServer server)
    {
        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                        <InitiateMultipartUploadResult>
                            <Bucket>test-bucket</Bucket>
                            <Key>test-document.pdf</Key>
                            <UploadId>upload-id-49e18525de9c</UploadId>
                        </InitiateMultipartUploadResult>";

        server
            .Given(Request.Create()
                .WithPath("/test-bucket/test-document.pdf")
                .UsingPost()
                .WithParam("uploads"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }

    private static void ConfigureUploadPartRequest(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/test-bucket/test-document.pdf")
                .UsingPut()
                .WithParam("partNumber", "2")
                .WithParam("uploadId", "upload-id-49e18525de9c"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("ETag", "etag-12345"));
    }

    private static void ConfigureCompleteUploadRequest(WireMockServer server)
    {
        var request = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                        <CompleteMultipartUpload>
                            <Part>
                                <ETag>etag-12345</ETag>
                                <PartNumber>1</PartNumber>
                            </Part>
                        </CompleteMultipartUpload>";

        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                        <CompleteMultipartUploadResult>
                            <Location>https://test-bucket.s3.amazonaws.com/test-document.pdf</Location>
                            <Bucket>test-bucket</Bucket>
                            <Key>test-document.pdf</Key>
                        </CompleteMultipartUploadResult>";

        server
            .Given(Request.Create()
                .WithPath("/test-bucket/test-document.pdf")
                .UsingPost()
                .WithParam("uploadId", "upload-id-49e18525de9c")
                .WithBody(request))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }
}
using CPS.ComplexCases.WireMock.Core;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.WireMock.Mappings;

public class UploadMapping : IWireMockMapping
{
    private const string TestDocumentFilePath = "/test-bucket/test-document.pdf";
    private const string ExistingDocumentFilePath = "/test-bucket/existing-document.pdf";

    public void Configure(WireMockServer server)
    {
        ConfigureUploadRequest(server);
        ConfigureUploadPartRequest(server);
        ConfigureCompleteUploadRequest(server);
        ConfigureGetExistingObjectRequest(server);
        ConfigureGetObjectAttributesRequest(server);
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
                .WithPath(TestDocumentFilePath)
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
                .WithPath(TestDocumentFilePath)
                .UsingPut()
                .WithParam("partNumber", new WildcardMatcher("*"))
                .WithParam("uploadId", "upload-id-49e18525de9c"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("ETag", "etag-12345"));
    }

    private static void ConfigureCompleteUploadRequest(WireMockServer server)
    {
        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                        <CompleteMultipartUploadResult>
                            <Location>https://test-bucket.s3.amazonaws.com/test-document.pdf</Location>
                            <Bucket>test-bucket</Bucket>
                            <Key>test-document.pdf</Key>
                        </CompleteMultipartUploadResult>";

        server
            .Given(Request.Create()
                .WithPath(TestDocumentFilePath)
                .UsingPost()
                .WithParam("uploadId", "upload-id-49e18525de9c"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }

    private static void ConfigureGetExistingObjectRequest(WireMockServer server)
    {
        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                        <GetObjectAttributesResult>
                            <ETag>etag-12345</ETag>
                        </GetObjectAttributesResult>";

        server
            .Given(Request.Create()
                .WithPath(ExistingDocumentFilePath)
                .UsingGet()
                .WithParam("attributes"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }

    private static void ConfigureGetObjectAttributesRequest(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath(TestDocumentFilePath)
                .UsingGet()
                .WithParam("attributes"))
            .RespondWith(Response.Create()
                .WithStatusCode(404));
    }
}
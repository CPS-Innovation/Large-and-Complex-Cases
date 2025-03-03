using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.WireMock.Mappings;

public class ObjectMapping : IWireMockMapping
{
    private const string delimiter = "/";

    public void Configure(WireMockServer server)
    {
        ConfigureUploadObjectMapping(server);
        ConfigureListObjectsMapping(server);
        ConfigureListNestedObjectsMapping(server);
        ConfigureGetObjectMapping(server);
    }

    private static void ConfigureUploadObjectMapping(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/test-bucket/test-file.txt")
                .UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("true"));
    }

    private static void ConfigureListObjectsMapping(WireMockServer server)
    {
        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                             <ListBucketResult>
                                <Name>test-bucket</Name>
                                <Prefix></Prefix>
                                <Marker></Marker>
                                <MaxKeys>1000</MaxKeys>
                                <IsTruncated>false</IsTruncated>
                                <Contents>
                                    <Key>test-file.txt</Key>
                                    <LastModified>2025-01-11T13:32:47+00:00</LastModified>
                                    <ETag>""70ee1738b6b21e2c8a43f3a5ab0eee71""</ETag>
                                    <Size>11</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                                <Contents>
                                    <Key>evidence.docx</Key>
                                    <LastModified>2025-01-30T10:17:35+00:00</LastModified>
                                    <ETag>""38b6b270ee171e2c8a40eee713f3a5ab""</ETag>
                                    <Size>434234</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                             </ListBucketResult>";

        server
            .Given(Request.Create()
                .WithPath("/test-bucket/")
                .UsingGet()
                .WithParam("list-type", "2"))
           .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }

    private static void ConfigureListNestedObjectsMapping(WireMockServer server)
    {
        var response = @"<?xml version=""1.0\"" encoding=""UTF-8""?>
                             <ListBucketResult>
                                <Name>nested-objects</Name>
                                <Prefix>/</Prefix>
                                <CommonPrefixes>
                                    <Prefix>counsel/</Prefix>
                                </CommonPrefixes>
                                <CommonPrefixes>
                                    <Prefix>counsel/statements/</Prefix>
                                </CommonPrefixes>
                                <CommonPrefixes>
                                    <Prefix>multimedia/</Prefix>
                                </CommonPrefixes>
                                <Marker></Marker>
                                <MaxKeys>1000</MaxKeys>
                                <IsTruncated>false</IsTruncated>
                                <Contents>
                                    <Key>counsel/test-file.txt</Key>
                                    <LastModified>2025-01-11T13:32:47+00:00</LastModified>
                                    <ETag>""70ee1738b6b21e2c8a43f3a5ab0eee71""</ETag>
                                    <Size>11</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                                <Contents>
                                    <Key>counsel/evidence.docx</Key>
                                    <LastModified>2025-01-30T10:17:35+00:00</LastModified>
                                    <ETag>""38b6b270ee171e2c8a40eee713f3a5ab""</ETag>
                                    <Size>434234</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                                <Contents>
                                    <Key>counsel/statements/statement.docx</Key>
                                    <LastModified>2025-01-30T10:17:35+00:00</LastModified>
                                    <ETag>""38b6b270ee171e2c8a40eee713f3a5ab""</ETag>
                                    <Size>434234</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                                <Contents>
                                    <Key>multimedia/dashcam.mp4</Key>
                                    <LastModified>2025-01-30T10:17:35+00:00</LastModified>
                                    <ETag>""38b6b270ee171e2c8a40eee713f3a5ab""</ETag>
                                    <Size>434234</Size>
                                    <StorageClass>STANDARD</StorageClass>
                                </Contents>
                             </ListBucketResult>";

        server
            .Given(Request.Create()
                .WithPath("/nested-objects/")
                .UsingGet()
                .WithParam("delimiter", "/")
                .WithParam("list-type", "2"))
           .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }

    private static void ConfigureGetObjectMapping(WireMockServer server)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test-document.pdf");
        var fileBytes = File.ReadAllBytes(filePath);

        server
            .Given(Request.Create()
                .WithPath("/test-bucket/test-document.pdf")
                .UsingGet())
           .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithHeader("Content-Length", fileBytes.Length.ToString())
                .WithHeader("Content-Disposition", "attachment; filename=test-document.pdf")
                .WithHeader("ETag", "70ee1738b6b21e2c8a43f3a5ab0eee71")
                .WithHeader("Last-Modified", "2025-02-20T13:32:47+00:00")
                .WithHeader("x-amz-id-2", "eftixk72aD6Ap51TnqcoF8eFidJG9Z/2mkiDFu8yU9AS1ed4OpIszj7UDNEHGran")
                .WithHeader("x-amz-request-id", "318BC8BC148832E5")
                .WithBody(fileBytes));
    }
}
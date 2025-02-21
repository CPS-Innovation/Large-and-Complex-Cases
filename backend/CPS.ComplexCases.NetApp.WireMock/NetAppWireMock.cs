using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace CPS.ComplexCases.NetApp.WireMock
{
    public class NetAppWireMock : INetAppWireMock
    {
        private readonly WireMockServer _server;

        public NetAppWireMock(IOptions<WireMockServerSettings> settings)
        {
            _server = WireMockServer.Start(settings.Value);
        }

        public void ConfigureMappings()
        {
            ConfigureCreateBucketMapping();
            ConfigureGetBucketAclWhereBucketDoesNotExistMapping();
            ConfigureFindBucketMapping();
            ConfigureUploadObjectMapping();
            ConfigureListObjectsMapping();
            ConfigureGetObjectMapping();
        }

        private void ConfigureCreateBucketMapping()
        {
            _server
                .Given(Request.Create()
                    .WithPath("/test-bucket/")
                    .UsingPut())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("true"));
        }

        private void ConfigureGetBucketAclWhereBucketDoesNotExistMapping()
        {
            var response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                             <Error>
                                <Code>NoSuchBucket</Code>
                                <Message>The specified bucket does not exist</Message>
                                <BucketName>test-bucket</BucketName>
                                <RequestId>318BC8BC148832E5</RequestId>
                                <HostId>eftixk72aD6Ap51TnqcoF8eFidJG9Z/2mkiDFu8yU9AS1ed4OpIszj7UDNEHGran</HostId>
                            </Error>";

            _server
                .Given(Request.Create()
                    .WithPath("/test-bucket/")
                    .UsingGet()
                    .WithParam("acl"))
                .RespondWith(Response.Create()
                    .WithStatusCode(404)
                    .WithBody(response));
        }

        private void ConfigureFindBucketMapping()
        {
            var response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                             <ListAllMyBucketsResult>
                                <Buckets>
                                    <Bucket>
                                        <CreationDate>2025-01-11T23:32:47+00:00</CreationDate>
                                        <Name>test-bucket</Name>
                                        <BucketRegion>eu-west-2</BucketRegion>
                                    </Bucket>
                                    <Bucket>
                                        <CreationDate>2025-02-10T23:32:13+00:00</CreationDate>
                                        <Name>Thunderstruck</Name>
                                        <BucketRegion>eu-west-2</BucketRegion>
                                    </Bucket>
                                </Buckets>
                                <Owner>
                                    <DisplayName>CPS+User</DisplayName>
                                    <ID>AIDACKCEVSQ6C2EXAMPLE</ID>
                                </Owner>
                                <ContinuationToken>eyJNYXJrZXIiOiBudWxsLCAiYm90b190cnVuY2F0ZV9hbW91bnQiOiAxfQ==</ContinuationToken>
                            </ListAllMyBucketsResult>";

            _server
                .Given(Request.Create()
                    .WithPath("/")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(response));
        }

        private void ConfigureUploadObjectMapping()
        {
            _server
                .Given(Request.Create()
                    .WithPath("/test-bucket/test-file.txt")
                    .UsingPut())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("true"));
        }

        private void ConfigureListObjectsMapping()
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

            _server
                .Given(Request.Create()
                    .WithPath("/test-bucket/")
                    .UsingGet()
                    .WithParam("list-type", "2"))
               .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(response));
        }

        private void ConfigureGetObjectMapping()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test-document.pdf");
            var fileBytes = File.ReadAllBytes(filePath);

            _server
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

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
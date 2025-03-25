using CPS.ComplexCases.WireMock.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CPS.ComplexCases.NetApp.WireMock.Mappings;

public class BucketMapping : IWireMockMapping
{
    public void Configure(WireMockServer server)
    {
        ConfigureCreateBucketMapping(server);
        ConfigureGetBucketAclWhereBucketDoesNotExistMapping(server);
        ConfigureFindBucketMapping(server);
    }

    private static void ConfigureCreateBucketMapping(WireMockServer server)
    {
        server
            .Given(Request.Create()
                .WithPath("/test-bucket/")
                .UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("true"));
    }

    private static void ConfigureGetBucketAclWhereBucketDoesNotExistMapping(WireMockServer server)
    {
        var response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                             <Error>
                                <Code>NoSuchBucket</Code>
                                <Message>The specified bucket does not exist</Message>
                                <BucketName>test-bucket</BucketName>
                                <RequestId>318BC8BC148832E5</RequestId>
                                <HostId>eftixk72aD6Ap51TnqcoF8eFidJG9Z/2mkiDFu8yU9AS1ed4OpIszj7UDNEHGran</HostId>
                            </Error>";

        server
            .Given(Request.Create()
                .WithPath("/test-bucket/")
                .UsingGet()
                .WithParam("acl"))
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBody(response));
    }

    private static void ConfigureFindBucketMapping(WireMockServer server)
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

        server
            .Given(Request.Create()
                .WithPath("/")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(response));
    }
}
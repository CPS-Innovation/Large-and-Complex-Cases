using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Constants;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppMockHttpRequestFactory : INetAppMockHttpRequestFactory
{
    public HttpRequestMessage CreateBucketRequest(CreateBucketArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, arg.BucketName);
        return request;
    }

    public HttpRequestMessage ListBucketsRequest(ListBucketsArg arg)
    {
        var query = new FormUrlEncodedContent(
        [
            new(S3Constants.ContinuationTokenQueryName, arg.ContinuationToken ?? string.Empty),
            new(S3Constants.MaxBucketsQueryName, arg.MaxBuckets.ToString() ?? "1000"),
            new(S3Constants.PrefixQueryName, arg.Prefix ?? string.Empty)
        ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"?{query.ReadAsStringAsync().Result}");

        return request;
    }

    public HttpRequestMessage FindBucketRequest(FindBucketArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, arg.BucketName);
        return request;
    }

    public HttpRequestMessage GetACLForBucketRequest(string bucketName)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{bucketName}/acl");
        return request;
    }

    public HttpRequestMessage UploadObjectRequest(UploadObjectArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{arg.BucketName}/{arg.ObjectKey}")
        {
            Content = new StreamContent(arg.Stream)
        };
        return request;
    }

    public HttpRequestMessage GetObjectRequest(GetObjectArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, arg.ObjectKey);
        request.Headers.Add(S3Constants.HostHeaderName, arg.BucketName);

        return request;
    }

    public HttpRequestMessage ListObjectsInBucketRequest(ListObjectsInBucketArg arg)
    {
        var query = new FormUrlEncodedContent(
        [
            new(S3Constants.ListTypeQueryName, "2"),
            new(S3Constants.ContinuationTokenQueryName, arg.ContinuationToken ?? string.Empty),
            new(S3Constants.DelimiterQueryValue, S3Constants.Delimiter),
            new(S3Constants.MaxKeysQueryName, arg.MaxKeys ?? string.Empty),
            new(S3Constants.PrefixQueryName, arg.Prefix ?? string.Empty),
        ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/{arg.BucketName}/?{query.ReadAsStringAsync().Result}");

        return request;
    }

    public HttpRequestMessage ListFoldersInBucketRequest(ListFoldersInBucketArg arg)
    {
        var query = new FormUrlEncodedContent(
        [
            new(S3Constants.ListTypeQueryName, "2"),
            new(S3Constants.ContinuationTokenQueryName, arg.ContinuationToken ?? string.Empty),
            new(S3Constants.DelimiterQueryValue, S3Constants.Delimiter),
            new(S3Constants.MaxKeysQueryName, arg.MaxKeys ?? string.Empty),
            new(S3Constants.PrefixQueryName, arg.Prefix ?? string.Empty)
        ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/{arg.BucketName}/?{query.ReadAsStringAsync().Result}");

        return request;
    }

    public HttpRequestMessage CreateMultipartUploadRequest(InitiateMultipartUploadArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{arg.BucketName}/{arg.ObjectKey}?uploads");

        request.Headers.Add(S3Constants.HostHeaderName, arg.BucketName);

        return request;
    }

    public HttpRequestMessage UploadPartRequest(UploadPartArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{arg.BucketName}/{arg.ObjectKey}?partNumber={arg.PartNumber}&uploadId={arg.UploadId}")
        {
            Content = new ByteArrayContent(arg.PartData)
        };

        return request;
    }

    public HttpRequestMessage CompleteMultipartUploadRequest(CompleteMultipartUploadArg arg)
    {
        var body = new StringContent(
            $"<CompleteMultipartUpload xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">{string.Join("", arg.CompletedParts.Select(p => $"<Part><PartNumber>{p.PartNumber}</PartNumber><ETag>{p.ETag}</ETag></Part>"))}</CompleteMultipartUpload>",
            System.Text.Encoding.UTF8,
            "application/xml"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, $"{arg.BucketName}/{arg.ObjectKey}?uploadId={arg.UploadId}")
        {
            Content = body
        };

        return request;
    }

    public HttpRequestMessage GetObjectAttributesRequest(GetObjectArg arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{arg.BucketName}/{arg.ObjectKey}?attributes");
        request.Headers.Add(S3Constants.HostHeaderName, arg.BucketName);
        request.Headers.Add(S3Constants.ObjectAttributesHeaderName, "ETag");

        return request;
    }
}

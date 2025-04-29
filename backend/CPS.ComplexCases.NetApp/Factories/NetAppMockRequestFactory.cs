using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Constants;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppMockRequestFactory : INetAppMockRequestFactory
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
        var request = new HttpRequestMessage(HttpMethod.Put, $"{arg.BucketName}/{arg.ObjectKey}");
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
        string? prefix = null;
        if (!string.IsNullOrEmpty(arg.Prefix))
        {
            prefix = !arg.Prefix.EndsWith(S3Constants.Delimiter) ? $"{arg.Prefix}{S3Constants.Delimiter}" : arg.Prefix;
        }

        var query = new FormUrlEncodedContent(
        [
            new(S3Constants.ListTypeQueryName, "2"),
            new(S3Constants.ContinuationTokenQueryName, arg.ContinuationToken ?? string.Empty),
            new(S3Constants.DelimiterQueryValue, S3Constants.Delimiter),
            new(S3Constants.MaxKeysQueryName, arg.MaxKeys ?? string.Empty),
            new(S3Constants.PrefixQueryName, prefix)
        ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"?{query.ReadAsStringAsync().Result}");

        return request;
    }

    public HttpRequestMessage ListFoldersInBucketRequest(ListFoldersInBucketArg arg)
    {
        string? prefix = null;
        if (!string.IsNullOrEmpty(arg.Prefix))
        {
            prefix = !arg.Prefix.EndsWith(S3Constants.Delimiter) ? $"{arg.Prefix}{S3Constants.Delimiter}" : arg.Prefix;
        }

        var query = new FormUrlEncodedContent(
        [
            new(S3Constants.ListTypeQueryName, "2"),
            new(S3Constants.ContinuationTokenQueryName, arg.ContinuationToken ?? string.Empty),
            new(S3Constants.DelimiterQueryValue, S3Constants.Delimiter),
            new(S3Constants.MaxKeysQueryName, arg.MaxKeys ?? string.Empty),
            new(S3Constants.PrefixQueryName, prefix)
        ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/{arg.BucketName}/?{query.ReadAsStringAsync().Result}");

        return request;
    }
}

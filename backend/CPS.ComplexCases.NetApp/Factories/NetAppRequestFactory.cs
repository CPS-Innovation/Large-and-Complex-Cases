using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppRequestFactory : INetAppRequestFactory
{
    public PutBucketRequest CreateBucketRequest(CreateBucketArg arg)
    {
        return new PutBucketRequest
        {
            BucketName = arg.BucketName,
            UseClientRegion = true
        };
    }

    public GetObjectRequest GetObjectRequest(GetObjectArg arg)
    {
        return new GetObjectRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
        };
    }

    public ListBucketsRequest ListBucketsRequest(ListBucketsArg arg)
    {
        return new ListBucketsRequest
        {
            ContinuationToken = arg.ContinuationToken,
            MaxBuckets = arg.MaxBuckets ?? 10000,
            Prefix = arg.Prefix
        };
    }

    public ListObjectsV2Request ListFoldersInBucketRequest(ListFoldersInBucketArg arg)
    {
        return new ListObjectsV2Request
        {
            BucketName = arg.BucketName,
            ContinuationToken = arg.ContinuationToken,
            Delimiter = S3Constants.Delimiter,
            Prefix = arg.Prefix,
        };
    }

    public ListObjectsV2Request ListObjectsInBucketRequest(ListObjectsInBucketArg arg)
    {
        return new ListObjectsV2Request
        {
            BucketName = arg.BucketName,
            ContinuationToken = arg.ContinuationToken,
            MaxKeys = !string.IsNullOrEmpty(arg.MaxKeys) ? int.Parse(arg.MaxKeys) : 1000,
            Delimiter = S3Constants.Delimiter,
            Prefix = arg.Prefix,
        };
    }

    public PutObjectRequest UploadObjectRequest(UploadObjectArg arg)
    {
        return new PutObjectRequest
        {
            BucketName = arg.BucketName,
            Key = arg.ObjectKey,
            InputStream = arg.Stream,
        };
    }
}
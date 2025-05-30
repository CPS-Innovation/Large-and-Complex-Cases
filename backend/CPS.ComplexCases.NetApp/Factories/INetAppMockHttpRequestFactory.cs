using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public interface INetAppMockHttpRequestFactory
    {
        HttpRequestMessage CreateBucketRequest(CreateBucketArg arg);
        HttpRequestMessage ListBucketsRequest(ListBucketsArg arg);
        HttpRequestMessage FindBucketRequest(FindBucketArg arg);
        HttpRequestMessage GetACLForBucketRequest(string bucketName);
        HttpRequestMessage UploadObjectRequest(UploadObjectArg arg);
        HttpRequestMessage GetObjectRequest(GetObjectArg arg);
        HttpRequestMessage ListObjectsInBucketRequest(ListObjectsInBucketArg arg);
        HttpRequestMessage ListFoldersInBucketRequest(ListFoldersInBucketArg arg);
        HttpRequestMessage CreateMultipartUploadRequest(InitiateMultipartUploadArg arg);
        HttpRequestMessage UploadPartRequest(UploadPartArg arg);
        HttpRequestMessage CompleteMultipartUploadRequest(CompleteMultipartUploadArg arg);
    }
}
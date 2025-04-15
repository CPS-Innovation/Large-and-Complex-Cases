using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public interface INetAppMockRequestFactory
    {
        HttpRequestMessage CreateBucketRequest(CreateBucketArg arg);
        HttpRequestMessage ListBucketsRequest(ListBucketsArg arg);
        HttpRequestMessage FindBucketRequest(FindBucketArg arg);
        HttpRequestMessage GetACLForBucketRequest(string bucketName);
        HttpRequestMessage UploadObjectRequest(UploadObjectArg arg);
        HttpRequestMessage GetObjectRequest(GetObjectArg arg);
        HttpRequestMessage ListObjectsInBucketRequest(ListObjectsInBucketArg arg);
        HttpRequestMessage ListFoldersInBucketRequest(ListFoldersInBucketArg arg);
    }
}
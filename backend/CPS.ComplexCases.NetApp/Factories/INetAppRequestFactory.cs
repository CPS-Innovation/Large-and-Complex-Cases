using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public interface INetAppRequestFactory
{
    PutBucketRequest CreateBucketRequest(CreateBucketArg arg);
    ListBucketsRequest ListBucketsRequest(ListBucketsArg arg);
    PutObjectRequest UploadObjectRequest(UploadObjectArg arg);
    GetObjectRequest GetObjectRequest(GetObjectArg arg);
    ListObjectsV2Request ListObjectsInBucketRequest(ListObjectsInBucketArg arg);
    ListObjectsV2Request ListFoldersInBucketRequest(ListFoldersInBucketArg arg);
    InitiateMultipartUploadRequest CreateMultipartUploadRequest(InitiateMultipartUploadArg arg);
    UploadPartRequest UploadPartRequest(UploadPartArg arg);
    CompleteMultipartUploadRequest CompleteMultipartUploadRequest(CompleteMultipartUploadArg arg);
    GetObjectAttributesRequest GetObjectAttributesRequest(GetObjectArg arg);
}
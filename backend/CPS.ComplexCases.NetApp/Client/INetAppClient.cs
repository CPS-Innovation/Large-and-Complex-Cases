using System.Net;
using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Client
{
    public interface INetAppClient
    {
        Task<bool> CreateBucketAsync(CreateBucketArg arg);
        Task<S3Bucket?> FindBucketAsync(FindBucketArg arg);
        Task<S3AccessControlList?> GetACLForBucketAsync(string bucketName);
        Task<bool> UploadObjectAsync(UploadObjectArg arg);
        Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg);
        Task<ListObjectsV2Response?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg);
    }
}
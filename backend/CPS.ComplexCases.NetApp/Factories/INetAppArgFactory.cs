using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public interface INetAppArgFactory
    {
        CreateBucketArg CreateCreateBucketArg(string bucketName);
        ListBucketsArg CreateListBucketsArg(string? continuationToken = null, int? maxBuckets = null, string? prefix = null);
        FindBucketArg CreateFindBucketArg(string bucketName);
        GetObjectArg CreateGetObjectArg(string bucketName, string objectName);
        UploadObjectArg CreateUploadObjectArg(string bucketName, string objectName, Stream stream);
        ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null);
        ListFoldersInBucketArg CreateListFoldersInBucketArg(string bucketName, string? operationName = null, string? continuationToken = null, int? maxKeys = null, string? prefix = null);
    }
}
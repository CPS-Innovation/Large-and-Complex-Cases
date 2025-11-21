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
        ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null, bool includeDelimiter = false);
        ListFoldersInBucketArg CreateListFoldersInBucketArg(string bucketName, string? operationName = null, string? continuationToken = null, int? maxKeys = null, string? prefix = null);
        InitiateMultipartUploadArg CreateInitiateMultipartUploadArg(string bucketName, string objectName);
        UploadPartArg CreateUploadPartArg(string bucketName, string objectName, byte[] partData, int partNumber, string uploadId);
        CompleteMultipartUploadArg CreateCompleteMultipartUploadArg(string bucketName, string objectName, string uploadId, Dictionary<int, string> parts);
        RegisterUserArg CreateRegisterUserArg(string username, string accessToken, Guid s3ServiceUuid);
        RegenerateUserKeysArg CreateRegenerateUserKeysArg(string username, string accessToken, Guid s3ServiceUuid);
    }
}
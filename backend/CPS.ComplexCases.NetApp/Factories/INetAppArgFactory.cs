using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public interface INetAppArgFactory
    {
        CreateBucketArg CreateCreateBucketArg(string bearerToken, string bucketName);
        ListBucketsArg CreateListBucketsArg(string bearerToken, string? continuationToken = null, int? maxBuckets = null, string? prefix = null);
        FindBucketArg CreateFindBucketArg(string bearerToken, string bucketName);
        GetObjectArg CreateGetObjectArg(string bearerToken, string bucketName, string objectName);
        UploadObjectArg CreateUploadObjectArg(string bearerToken, string bucketName, string objectName, Stream stream, long contentLength, bool disablePayloadSigning = true);
        ListObjectsInBucketArg CreateListObjectsInBucketArg(string bearerToken, string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null, bool includeDelimiter = false);
        ListFoldersInBucketArg CreateListFoldersInBucketArg(string bearerToken, string bucketName, string? operationName = null, string? continuationToken = null, int? maxKeys = null, string? prefix = null);
        InitiateMultipartUploadArg CreateInitiateMultipartUploadArg(string bearerToken, string bucketName, string objectName);
        UploadPartArg CreateUploadPartArg(string bearerToken, string bucketName, string objectName, byte[] partData, int partNumber, string uploadId);
        CompleteMultipartUploadArg CreateCompleteMultipartUploadArg(string bearerToken, string bucketName, string objectName, string uploadId, Dictionary<int, string> parts);
        RegisterUserArg CreateRegisterUserArg(string username, string accessToken, Guid s3ServiceUuid);
        RegenerateUserKeysArg CreateRegenerateUserKeysArg(string username, string accessToken, Guid s3ServiceUuid);
        DeleteFileOrFolderArg CreateDeleteFileOrFolderArg(string bearerToken, string bucketName, string operationName, string path);
    }
}
using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.NetApp.Client
{
    public interface INetAppClient
    {
        Task<bool> CreateBucketAsync(CreateBucketArg arg);
        Task<IEnumerable<S3Bucket>> ListBucketsAsync(ListBucketsArg arg);
        Task<S3Bucket?> FindBucketAsync(FindBucketArg arg);
        Task<bool> CreateFolderAsync(CreateFolderArg arg);
        Task<bool> UploadObjectAsync(UploadObjectArg arg);
        Task<GetObjectResponse?> GetObjectAsync(GetObjectArg arg);
        Task<ListNetAppObjectsDto?> ListObjectsInBucketAsync(ListObjectsInBucketArg arg);
        Task<ListNetAppObjectsDto?> ListFoldersInBucketAsync(ListFoldersInBucketArg arg);
        Task<InitiateMultipartUploadResponse?> InitiateMultipartUploadAsync(InitiateMultipartUploadArg arg);
        Task<UploadPartResponse?> UploadPartAsync(UploadPartArg arg);
        Task<CompleteMultipartUploadResponse?> CompleteMultipartUploadAsync(CompleteMultipartUploadArg arg, CancellationToken cancellationToken = default);
        Task<bool> DoesObjectExistAsync(GetObjectArg arg);
        Task<string> DeleteFileOrFolderAsync(DeleteFileOrFolderArg arg);
    }
}
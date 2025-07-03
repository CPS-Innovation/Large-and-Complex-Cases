using Amazon.S3.Model;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public class NetAppArgFactory : INetAppArgFactory
    {
        public CreateBucketArg CreateCreateBucketArg(string bucketName)
        {
            return new CreateBucketArg
            {
                BucketName = bucketName.ToLowerInvariant()
            };
        }

        public ListBucketsArg CreateListBucketsArg(string? continuationToken = null, int? maxBuckets = null, string? prefix = null)
        {
            return new ListBucketsArg
            {
                ContinuationToken = continuationToken,
                MaxBuckets = maxBuckets,
                Prefix = prefix
            };
        }

        public FindBucketArg CreateFindBucketArg(string bucketName)
        {
            return new FindBucketArg
            {
                BucketName = bucketName.ToLowerInvariant()
            };
        }

        public GetObjectArg CreateGetObjectArg(string bucketName, string objectName)
        {
            return new GetObjectArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ObjectKey = objectName
            };
        }

        public UploadObjectArg CreateUploadObjectArg(string bucketName, string objectName, Stream stream)
        {
            return new UploadObjectArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ObjectKey = objectName,
                Stream = stream
            };
        }

        public ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null, bool includeDelimiter = false)
        {
            return new ListObjectsInBucketArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ContinuationToken = continuationToken,
                MaxKeys = maxKeys.ToString(),
                Prefix = SetPrefix(prefix),
                IncludeDelimiter = includeDelimiter
            };
        }

        public ListFoldersInBucketArg CreateListFoldersInBucketArg(string bucketName, string? operationName = null, string? continuationToken = null, int? maxKeys = null, string? prefix = null)
        {
            return new ListFoldersInBucketArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                OperationName = operationName?.ToLowerInvariant(),
                ContinuationToken = continuationToken,
                MaxKeys = maxKeys.ToString(),
                Prefix = SetPrefix(prefix)
            };
        }

        private static string SetPrefix(string? prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                prefix = !prefix.EndsWith(S3Constants.Delimiter) ? $"{prefix}{S3Constants.Delimiter}" : prefix;
            }

            return prefix ?? string.Empty;
        }

        public InitiateMultipartUploadArg CreateInitiateMultipartUploadArg(string bucketName, string objectName)
        {
            return new InitiateMultipartUploadArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ObjectKey = objectName
            };
        }

        public UploadPartArg CreateUploadPartArg(string bucketName, string objectName, byte[] partData, int partNumber, string uploadId)
        {
            return new UploadPartArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ObjectKey = objectName,
                PartData = partData,
                PartNumber = partNumber,
                UploadId = uploadId
            };
        }

        public CompleteMultipartUploadArg CreateCompleteMultipartUploadArg(string bucketName, string objectName, string uploadId, Dictionary<int, string> parts)
        {
            return new CompleteMultipartUploadArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ObjectKey = objectName,
                UploadId = uploadId,
                CompletedParts = parts.Select(part => new PartETag
                {
                    PartNumber = part.Key,
                    ETag = part.Value
                }).ToList()
            };
        }
    }
}
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

        public ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string? continuationToken = null, int? maxKeys = null, string? prefix = null)
        {
            return new ListObjectsInBucketArg
            {
                BucketName = bucketName.ToLowerInvariant(),
                ContinuationToken = continuationToken,
                MaxKeys = maxKeys.ToString(),
                Prefix = SetPrefix(prefix)
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
                Prefix = SetPrefix(prefix, true)
            };
        }

        private static string SetPrefix(string? prefix, bool ensureTrailingDelimiter = false)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                if (ensureTrailingDelimiter && !prefix.EndsWith(S3Constants.Delimiter))
                {
                    prefix = $"{prefix}{S3Constants.Delimiter}";
                }
                else if (!ensureTrailingDelimiter && prefix.EndsWith(S3Constants.Delimiter))
                {
                    prefix = prefix.TrimEnd(S3Constants.Delimiter[0]);
                }
            }

            return prefix?.ToLowerInvariant() ?? string.Empty;
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
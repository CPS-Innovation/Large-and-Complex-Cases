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
                Prefix = SetPrefix(prefix)
            };
        }

        private static string SetPrefix(string? prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                prefix = !prefix.EndsWith(S3Constants.Delimiter) ? $"{prefix}{S3Constants.Delimiter}" : prefix;
            }

            return prefix?.ToLowerInvariant() ?? string.Empty;
        }
    }
}
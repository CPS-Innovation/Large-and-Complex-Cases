using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public class NetAppArgFactory : INetAppArgFactory
    {
        public CreateBucketArg CreateCreateBucketArg(string bucketName)
        {
            return new CreateBucketArg
            {
                BucketName = bucketName
            };
        }

        public FindBucketArg CreateFindBucketArg(string bucketName)
        {
            return new FindBucketArg
            {
                BucketName = bucketName
            };
        }

        public GetObjectArg CreateGetObjectArg(string bucketName, string objectName)
        {
            return new GetObjectArg
            {
                BucketName = bucketName,
                ObjectName = objectName
            };
        }

        public UploadObjectArg CreateUploadObjectArg(string bucketName, string objectName, Stream stream)
        {
            return new UploadObjectArg
            {
                BucketName = bucketName,
                ObjectName = objectName,
                Stream = stream
            };
        }

        public ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string? continuationToken = null)
        {
            return new ListObjectsInBucketArg
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken
            };
        }

        public ListFoldersInBucketArg CreateListFoldersInBucketArg(string bucketName, string? continuationToken = null, string? prefix = null)
        {
            return new ListFoldersInBucketArg
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken,
                Prefix = prefix
            };
        }
    }
}
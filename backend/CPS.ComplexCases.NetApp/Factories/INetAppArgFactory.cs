using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories
{
    public interface INetAppArgFactory
    {
        CreateBucketArg CreateCreateBucketArg(string bucketName);
        FindBucketArg CreateFindBucketArg(string bucketName);
        GetObjectArg CreateGetObjectArg(string bucketName, string objectName);
        UploadObjectArg CreateUploadObjectArg(string bucketName, string objectName, Stream stream);
        ListObjectsInBucketArg CreateListObjectsInBucketArg(string bucketName, string continuationToken = "");
    }
}
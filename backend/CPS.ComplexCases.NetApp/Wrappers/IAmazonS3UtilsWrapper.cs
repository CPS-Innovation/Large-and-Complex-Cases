using Amazon.S3;

namespace CPS.ComplexCases.NetApp.Wrappers
{
    public interface IAmazonS3UtilsWrapper
    {
        Task<bool> DoesS3BucketExistV2Async(IAmazonS3 client, string bucketName);
    }
}
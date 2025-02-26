using Amazon.S3;
using Amazon.S3.Util;

namespace CPS.ComplexCases.NetApp.Wrappers
{
    public class AmazonS3UtilsWrapper : IAmazonS3UtilsWrapper
    {
        public virtual Task<bool> DoesS3BucketExistV2Async(IAmazonS3 client, string bucketName)
        {
            return AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
        }
    }
}
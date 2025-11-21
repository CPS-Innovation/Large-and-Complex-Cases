using Amazon.S3;

namespace CPS.ComplexCases.NetApp.Factories;

public interface IS3ClientFactory
{
    public Task<IAmazonS3> GetS3ClientAsync();
    public void SetS3ClientAsync(IAmazonS3 s3Client);
}
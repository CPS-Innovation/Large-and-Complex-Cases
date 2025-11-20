using Amazon.S3;

namespace CPS.ComplexCases.NetApp.Factories;

public interface IS3ClientFactory
{
    public Task<IAmazonS3> CreateS3ClientAsync();
}
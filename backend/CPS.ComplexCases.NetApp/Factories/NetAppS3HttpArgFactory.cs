using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppS3HttpArgFactory : INetAppS3HttpArgFactory
{
    public GetHeadObjectArg CreateGetHeadObjectArg(string bearerToken, string bucketName, string objectKey)
    {
        return new GetHeadObjectArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName,
            ObjectKey = objectKey
        };
    }
}
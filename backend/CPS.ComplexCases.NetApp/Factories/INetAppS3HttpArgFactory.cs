using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public interface INetAppS3HttpArgFactory
{
    GetHeadObjectArg CreateGetHeadObjectArg(string bearerToken, string bucketName, string objectKey);
}
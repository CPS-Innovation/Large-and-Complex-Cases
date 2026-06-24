using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public class OntapArgFactory : IOntapArgFactory
{
    public MaterialRenameArg CreateMaterialRenameArg(string bearerToken, Guid ontapVolumeUuid, string currentPath, string newPath)
    {
        return new MaterialRenameArg
        {
            BearerToken = bearerToken,
            OntapVolumeUuid = ontapVolumeUuid,
            CurrentFilePath = currentPath,
            NewFilePath = newPath
        };
    }

    public GetFileLockArg CreateGetFileLockArg(string bearerToken, string bucketName, Guid volumeUuid, string filePath)
    {
        return new GetFileLockArg
        {
            BearerToken = bearerToken,
            BucketName = bucketName,
            VolumeUuid = volumeUuid,
            FilePath = filePath
        };
    }

    public GetCifsSessionUserArg CreateGetCifsSessionUserArg(string bearerToken, string clientIp)
    {
        return new GetCifsSessionUserArg
        {
            BearerToken = bearerToken,
            ClientIp = clientIp,
        };
    }
}

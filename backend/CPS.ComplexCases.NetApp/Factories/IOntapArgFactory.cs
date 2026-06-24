using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public interface IOntapArgFactory
{
    MaterialRenameArg CreateMaterialRenameArg(string bearerToken, Guid ontapVolumeUuid, string currentPath, string newPath);
    GetFileLockArg CreateGetFileLockArg(string bearerToken, string bucketName, Guid ontapVolumeUuid, string filePath);
    GetCifsSessionUserArg CreateGetCifsSessionUserArg(string bearerToken, string clientIp);
}
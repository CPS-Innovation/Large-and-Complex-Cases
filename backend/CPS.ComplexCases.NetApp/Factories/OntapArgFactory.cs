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
            CurrentFolderPath = currentPath,
            NewFolderPath = newPath
        };
    }
}

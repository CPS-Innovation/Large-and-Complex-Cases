using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public interface IOntapArgFactory
{
    MaterialRenameArg CreateMaterialRenameArg(string bearerToken, Guid ontapVolumeUuid, string currentPath, string newPath);
}
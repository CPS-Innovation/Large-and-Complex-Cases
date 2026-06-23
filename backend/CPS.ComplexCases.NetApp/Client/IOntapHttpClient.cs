using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Client;

public interface IOntapHttpClient
{
    Task<MaterialRenameResult> RenameMaterialAsync(MaterialRenameArg arg);
    Task<GetFileLockResult> GetFileLockAsync(GetFileLockArg arg);
}
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Ontap;

namespace CPS.ComplexCases.NetApp.Client;

public interface IOntapHttpClient
{
    Task<MaterialRenameResult> RenameMaterialAsync(MaterialRenameArg arg);
    Task<GetFileLockProtocolResult> GetFileLockAsync(GetFileLockArg arg);
    Task<GetCifsSessionUserResult> GetCifsSessionUserAsync(GetCifsSessionUserArg arg);
}
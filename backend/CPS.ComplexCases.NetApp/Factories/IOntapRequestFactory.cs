using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Factories;

public interface IOntapRequestFactory
{
    HttpRequestMessage CreateRenameMaterialRequest(MaterialRenameArg arg);
    HttpRequestMessage CreateGetFileLockRequest(GetFileLockArg arg);
    HttpRequestMessage CreateGetCifsSessionUserRequest(GetCifsSessionUserArg arg);
}
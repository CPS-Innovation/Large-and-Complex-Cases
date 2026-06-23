using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
namespace CPS.ComplexCases.NetApp.Services;

public interface IOntapService
{
    Task<GetFileLockResult> GetFileLockAsync(GetFileLockArg arg);
}
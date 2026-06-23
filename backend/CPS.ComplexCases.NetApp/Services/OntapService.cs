using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;

namespace CPS.ComplexCases.NetApp.Services;

public class OntapService : IOntapService
{
    private readonly IOntapHttpClient _ontapHttpClient;

    public OntapService(IOntapHttpClient ontapHttpClient)
    {
        _ontapHttpClient = ontapHttpClient;
    }

    public async Task<GetFileLockResult> GetFileLockAsync(GetFileLockArg arg)
    {
        return await _ontapHttpClient.GetFileLockAsync(arg);
    }
}
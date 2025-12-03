using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Client;

public interface INetAppHttpClient
{
    public Task<NetAppUserResponse> RegisterUserAsync(RegisterUserArg arg);
    public Task<NetAppUserResponse> RegenerateUserKeysAsync(RegenerateUserKeysArg arg);
}
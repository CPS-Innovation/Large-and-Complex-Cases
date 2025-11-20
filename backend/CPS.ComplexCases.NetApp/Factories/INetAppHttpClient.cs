using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.NetApp;

namespace CPS.ComplexCases.NetApp.Factories;

public interface INetAppHttpClient
{
    public Task<NetAppUserResponse> RegisterUserAsync(RegisterUserArg arg);
    public Task<NetAppUserResponse> RegenerateUserKeysAsync(RegenerateUserKeysArg arg);
}
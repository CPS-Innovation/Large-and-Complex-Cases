namespace CPS.ComplexCases.Common.Services;

public class UserService : IUserService
{
    private string? _bearerToken;

    public Task SetUserBearerTokenAsync(string bearerToken)
    {
        _bearerToken = bearerToken;
        return Task.CompletedTask;
    }

    public Task<string?> GetUserBearerTokenAsync()
    {
        return Task.FromResult(_bearerToken);
    }
}
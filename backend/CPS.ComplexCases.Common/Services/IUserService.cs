namespace CPS.ComplexCases.Common.Services;

public interface IUserService
{
    Task SetUserBearerTokenAsync(string bearerToken);
    Task<string?> GetUserBearerTokenAsync();
}
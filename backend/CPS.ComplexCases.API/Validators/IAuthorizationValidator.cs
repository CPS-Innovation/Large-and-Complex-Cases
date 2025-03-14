namespace CPS.ComplexCases.API.Validators;

public interface IAuthorizationValidator
{
  Task<ValidateTokenResult> ValidateTokenAsync(string token, string? requiredScopes = null, string? requiredRoles = null);
}
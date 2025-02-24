using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Validators;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.API.Handlers;

public class InitializationHandler(IAuthorizationValidator tokenValidator) : IInitializationHandler
{
  private readonly IAuthorizationValidator _tokenValidator = tokenValidator;

  public async Task<ValidateTokenResult> Initialize(HttpRequest request)
  {
    return await AuthenticateRequest(request);
  }

  private async Task<ValidateTokenResult> AuthenticateRequest(HttpRequest request)
  {
    if (!request.Headers.TryGetValue("Authorization", out var accessTokenValue) ||
        string.IsNullOrWhiteSpace(accessTokenValue))
      throw new CpsAuthenticationException();

    var validateTokenResult = await _tokenValidator.ValidateTokenAsync(accessTokenValue!, "user_impersonation");
    if (!validateTokenResult.IsValid)
      throw new CpsAuthenticationException();

    return validateTokenResult;
  }
}
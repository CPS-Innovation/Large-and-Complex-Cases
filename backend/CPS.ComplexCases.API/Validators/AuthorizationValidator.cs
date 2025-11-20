using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CPS.ComplexCases.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CPS.ComplexCases.API.Validators;

public class AuthorizationValidator(ILogger<AuthorizationValidator> logger, ConfigurationManager<OpenIdConnectConfiguration> configurationManager, IUserService userService) : IAuthorizationValidator
{
  private readonly ILogger<AuthorizationValidator> _logger = logger;
  private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager = configurationManager;
  private readonly IUserService _userService = userService;
  private const string ScopeType = @"http://schemas.microsoft.com/identity/claims/scope";

  public async Task<ValidateTokenResult> ValidateTokenAsync(string token, string? requiredScopes = null, string? requiredRoles = null)
  {
    if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
    try
    {
      var audience = Environment.GetEnvironmentVariable("CallingAppValidAudience");
      var discoveryDocument = await _configurationManager.GetConfigurationAsync(default);

      var validationParameters = new TokenValidationParameters
      {
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ValidateIssuer = true,
        ValidIssuer = discoveryDocument.Issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = discoveryDocument.SigningKeys,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
        NameClaimType = "preferred_username",

      };

      var tokenValidator = new JwtSecurityTokenHandler();
      var tokenString = token
          .ToString()
          .Split(" ")
          .Last();
      var claimsPrincipal = tokenValidator.ValidateToken(tokenString, validationParameters, out _);

      var isValid = IsValid(claimsPrincipal, requiredScopes, requiredRoles);
      var username = claimsPrincipal?.Identity?.Name;

      await _userService.SetUserBearerTokenAsync(tokenString);

      return new ValidateTokenResult
      {
        IsValid = isValid,
        Username = username,
        Token = tokenString
      };
    }
    catch (InvalidOperationException ex)
    {
      _logger.LogError(ex, $"{nameof(ValidateTokenAsync)}: An invalid operation exception was caught");
      return new ValidateTokenResult
      {
        IsValid = false
      };
    }
    catch (SecurityTokenValidationException ex)
    {
      _logger.LogError(ex, $"{nameof(ValidateTokenAsync)}: A security exception was caught");
      return new ValidateTokenResult
      {
        IsValid = false,
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"{nameof(ValidateTokenAsync)}: An unexpected error was caught");
      return new ValidateTokenResult
      {
        IsValid = false,
      };
    }
  }

  private static bool IsValid(ClaimsPrincipal claimsPrincipal, string? scopes = null, string? roles = null)
  {
    if (claimsPrincipal == null)
    {
      return false;
    }

    var requiredScopes = LoadRequiredItems(scopes ?? string.Empty);
    var requiredRoles = LoadRequiredItems(roles ?? string.Empty);

    if (requiredScopes.Count == 0 && requiredRoles.Count == 0)
    {
      return true;
    }

    var hasAccessToRoles = requiredRoles.Count == 0 || requiredRoles.All(claimsPrincipal.IsInRole);
    var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == ScopeType)
        ? claimsPrincipal.Claims.First(x => x.Type == ScopeType).Value
        : string.Empty;

    var tokenScopes = scopeClaim.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
    var hasAccessToScopes = requiredScopes.Count == 0 || requiredScopes.All(x => tokenScopes.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

    return hasAccessToRoles && hasAccessToScopes;
  }

  private static List<string> LoadRequiredItems(string items)
  {
    return string.IsNullOrWhiteSpace(items)
        ? []
        : items.Replace(" ", string.Empty).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
  }
}
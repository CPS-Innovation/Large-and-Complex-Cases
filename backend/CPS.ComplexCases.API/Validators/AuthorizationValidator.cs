using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CPS.ComplexCases.API.Validators;

public class AuthorizationValidator(ILogger<AuthorizationValidator> logger, ConfigurationManager<OpenIdConnectConfiguration> configurationManager) : IAuthorizationValidator
{
  private readonly ILogger<AuthorizationValidator> _logger = logger;
  private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager = configurationManager;
  private const string ScopeType = @"http://schemas.microsoft.com/identity/claims/scope";
  private const string RolesType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

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

      // For app tokens, use app name or object ID; for user tokens, use preferred_username
      var username = GetIdentifier(claimsPrincipal);

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

  private bool IsValid(ClaimsPrincipal claimsPrincipal, string? scopes = null, string? roles = null)
  {
    if (claimsPrincipal == null)
    {
      return false;
    }

    var requiredScopes = LoadRequiredItems(scopes ?? string.Empty);
    var requiredRoles = LoadRequiredItems(roles ?? string.Empty);

    var isAppToken = IsApplicationToken(claimsPrincipal);

    if (isAppToken)
    {
      // For app tokens, validate against app roles only
      return ValidateAppToken(claimsPrincipal, requiredScopes, requiredRoles);
    }
    else
    {
      // For delegated tokens, validate against scopes (and optionally user roles)
      return ValidateDelegatedToken(claimsPrincipal, requiredScopes, requiredRoles);
    }
  }

  private static bool IsApplicationToken(ClaimsPrincipal principal)
  {
    var hasIdTypApp = principal.HasClaim("idtyp", "app");
    var hasRoles = principal.HasClaim(c => c.Type == RolesType);
    var hasScopes = principal.HasClaim(c => c.Type == ScopeType);

    // If both "scp" and "roles" exist, force delegated
    if (hasScopes && hasRoles) return false;

    return hasIdTypApp || (hasRoles && !hasScopes);
  }

  private bool ValidateAppToken(ClaimsPrincipal claimsPrincipal, List<string> requiredScopes, List<string> requiredRoles)
  {
    // For app tokens, map required scopes to app roles
    // Convention: scope "user_impersonation" maps to role "API.Access"

    var tokenRoles = claimsPrincipal.FindAll(RolesType)
        .Select(c => c.Value)
        .ToList();

    // If specific roles are required, check them
    if (requiredRoles.Count > 0)
    {
      var missingRoles = requiredRoles
          .Where(requiredRole => !tokenRoles.Any(tokenRole =>
              string.Equals(requiredRole, tokenRole, StringComparison.OrdinalIgnoreCase)))
          .ToList();

      if (missingRoles.Count > 0)
      {
        _logger.LogDebug(
            "App token missing required roles. Required: [{RequiredRoles}], Present: [{TokenRoles}], Missing: [{MissingRoles}]",
            string.Join(", ", requiredRoles),
            string.Join(", ", tokenRoles),
            string.Join(", ", missingRoles));
        return false;
      }
    }

    if (requiredScopes.Count > 0)
    {
      var mappedRoles = MapScopesToRoles(requiredScopes);
      var unmappedScopes = requiredScopes
          .Where((s, i) => string.Equals(mappedRoles[i], s, StringComparison.OrdinalIgnoreCase))
          .ToList();

      if (unmappedScopes.Count > 0)
        _logger.LogWarning("Unmapped scopes for app token: {Scopes}", string.Join(", ", unmappedScopes));

      var hasAll = mappedRoles.All(mr =>
          tokenRoles.Any(tr => string.Equals(mr, tr, StringComparison.OrdinalIgnoreCase)));

      if (!hasAll) return false;
    }

    return requiredRoles.Count == 0 && requiredScopes.Count == 0 ? tokenRoles.Count > 0 : true;
  }

  private static bool ValidateDelegatedToken(ClaimsPrincipal claimsPrincipal, List<string> requiredScopes, List<string> requiredRoles)
  {
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

  private static List<string> MapScopesToRoles(List<string> scopes)
  {
    // Map delegated permission scopes to application permission roles
    var scopeToRoleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "user_impersonation", "API.Access" }
        };

    return scopes
        .Select(scope => scopeToRoleMap.TryGetValue(scope, out var role) ? role : scope)
        .ToList();
  }

  private static string? GetIdentifier(ClaimsPrincipal claimsPrincipal)
  {
    // For user tokens: try multiple claims in order of preference
    // 1. Identity.Name (if NameClaimType is set correctly)
    var username = claimsPrincipal?.Identity?.Name;
    if (!string.IsNullOrWhiteSpace(username))
    {
      return username;
    }

    // 2. preferred_username (UPN for AAD users)
    var upn = claimsPrincipal?.FindFirst("preferred_username")?.Value;
    if (!string.IsNullOrWhiteSpace(upn))
    {
      return upn;
    }

    // 3. oid (Object ID - always present for AAD users)
    var oid = claimsPrincipal?.FindFirst("oid")?.Value;
    if (!string.IsNullOrWhiteSpace(oid))
    {
      return oid;
    }

    // For app tokens: use app display name or app ID
    var appId = claimsPrincipal?.FindFirst("appid")?.Value
             ?? claimsPrincipal?.FindFirst("azp")?.Value;

    if (!string.IsNullOrWhiteSpace(appId))
    {
      return $"[APP:{appId}]";
    }

    return null;
  }

  private static List<string> LoadRequiredItems(string items)
  {
    return string.IsNullOrWhiteSpace(items)
        ? []
        : items.Replace(" ", string.Empty).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
  }
}
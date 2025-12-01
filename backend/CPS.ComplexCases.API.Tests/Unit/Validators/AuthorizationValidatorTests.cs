namespace CPS.ComplexCases.API.Tests.Unit.Validators;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CPS.ComplexCases.API.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

public class AuthorizationValidatorTests
{
    private readonly Mock<ILogger<AuthorizationValidator>> _mockLogger;
    private readonly TestConfigurationManager _configManager;
    private readonly AuthorizationValidator _validator;
    private readonly string _validAudience = "test-audience";
    private readonly string _validIssuer = "https://login.microsoftonline.com/test-tenant/v2.0";
    private const string ScopeType = "http://schemas.microsoft.com/identity/claims/scope";
    private const string RolesType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    public AuthorizationValidatorTests()
    {
        _mockLogger = new Mock<ILogger<AuthorizationValidator>>();
        _configManager = new TestConfigurationManager();

        Environment.SetEnvironmentVariable("CallingAppValidAudience", _validAudience);

        _validator = new AuthorizationValidator(_mockLogger.Object, _configManager);
    }

    [Fact]
    public void IsApplicationToken_ReturnsTrue_WhenIdtypIsApp()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("idtyp", "app"),
            new Claim(RolesType, "API.Access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act - using reflection to test private method
        var result = InvokeIsApplicationToken(claimsPrincipal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsApplicationToken_ReturnsTrue_WhenHasRolesButNoScopes()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "API.Access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeIsApplicationToken(claimsPrincipal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsApplicationToken_ReturnsFalse_WhenHasScopesButNoRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeIsApplicationToken(claimsPrincipal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsApplicationToken_ReturnsFalse_WhenHasBothScopesAndRoles()
    {
        // POLICY: Tokens with BOTH "scp" and "roles" (e.g., on-behalf-of flows)
        // are treated as DELEGATED tokens. This ensures they are validated against
        // delegated permission scopes rather than application roles.

        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation"),
            new Claim(RolesType, "API.Access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeIsApplicationToken(claimsPrincipal);

        // Assert - Should be treated as delegated token
        Assert.False(result);
    }

    [Fact]
    public void IsApplicationToken_ReturnsFalse_WhenHasBothScopesAndRolesWithIdtypApp()
    {
        // POLICY: Even with idtyp=app, if both scopes and roles are present,
        // the token is treated as DELEGATED because the "both scopes and roles" check
        // takes precedence over the idtyp check in the implementation.

        // Arrange
        var claims = new List<Claim>
    {
        new Claim("idtyp", "app"),
        new Claim(ScopeType, "user_impersonation"),
        new Claim(RolesType, "API.Access")
    };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeIsApplicationToken(claimsPrincipal);

        // Assert - Should be treated as delegated token because both scopes and roles are present
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ThrowsArgumentNullException_WhenTokenIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _validator.ValidateTokenAsync(string.Empty));
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsInvalid_WhenConfigurationManagerThrowsException()
    {
        // Arrange
        _configManager.ShouldThrowException = true;

        // Act
        var result = await _validator.ValidateTokenAsync("Bearer test-token");

        // Assert
        Assert.False(result.IsValid);
        Assert.Null(result.Username);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsInvalid_WhenTokenValidationFails()
    {
        // Arrange
        var config = CreateMockOpenIdConnectConfiguration();
        _configManager.Configuration = config;

        // Act
        var result = await _validator.ValidateTokenAsync("Bearer invalid-token");

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsTrue_WhenNoRequirementsSpecified()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, new List<string>(), new List<string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsTrue_WhenRequiredScopesMatch()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation read write")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredScopes = new List<string> { "user_impersonation", "read" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, requiredScopes, new List<string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsFalse_WhenRequiredScopesMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredScopes = new List<string> { "user_impersonation", "admin" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, requiredScopes, new List<string>());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsTrue_WhenRequiredRolesMatch()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var requiredRoles = new List<string> { "Admin" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsFalse_WhenRequiredRolesMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var requiredRoles = new List<string> { "Admin" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateDelegatedToken_ReturnsTrue_WhenBothScopesAndRolesMatch()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "user_impersonation read"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var requiredScopes = new List<string> { "user_impersonation" };
        var requiredRoles = new List<string> { "Admin" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, requiredScopes, requiredRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDelegatedToken_IsCaseInsensitive_ForScopes()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ScopeType, "User_Impersonation READ")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredScopes = new List<string> { "user_impersonation", "read" };

        // Act
        var result = InvokeValidateDelegatedToken(claimsPrincipal, requiredScopes, new List<string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAppToken_ReturnsTrue_WhenTokenHasRolesAndNoRequirements()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "API.Access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), new List<string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAppToken_ReturnsFalse_WhenTokenHasNoRolesAndNoRequirements()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("appid", "test-app-id")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), new List<string>());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAppToken_ReturnsTrue_WhenRequiredRolesMatch()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "API.Access"),
            new Claim(RolesType, "Data.Read")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredRoles = new List<string> { "API.Access" };

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAppToken_ReturnsFalse_WhenRequiredRolesMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "Data.Read")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredRoles = new List<string> { "API.Access" };

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAppToken_MapsScopesToRoles_WhenScopesProvided()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "API.Access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredScopes = new List<string> { "user_impersonation" };

        // Act - user_impersonation scope should map to API.Access role
        var result = InvokeValidateAppToken(claimsPrincipal, requiredScopes, new List<string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAppToken_ReturnsFalse_WhenMappedRolesMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "Data.Read")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredScopes = new List<string> { "user_impersonation" };

        // Act - user_impersonation maps to API.Access which is missing
        var result = InvokeValidateAppToken(claimsPrincipal, requiredScopes, new List<string>());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateAppToken_IsCaseInsensitive_ForRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "api.access")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredRoles = new List<string> { "API.Access" };

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAppToken_LogsDebug_WhenRolesMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(RolesType, "Data.Read")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requiredRoles = new List<string> { "API.Access", "Admin.Write" };

        // Act
        var result = InvokeValidateAppToken(claimsPrincipal, new List<string>(), requiredRoles);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    (v.ToString() ?? string.Empty).Contains("App token missing required roles")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetIdentifier_ReturnsIdentityName_WhenPresent()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "user@example.com"),
            new Claim("oid", "123-456-789")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", "preferred_username", ClaimTypes.Role);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Equal("user@example.com", result);
    }

    [Fact]
    public void GetIdentifier_ReturnsPreferredUsername_WhenIdentityNameIsNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "user@example.com"),
            new Claim("oid", "123-456-789")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Equal("user@example.com", result);
    }

    [Fact]
    public void GetIdentifier_ReturnsOid_WhenPreferredUsernameIsNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("oid", "123-456-789")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Equal("123-456-789", result);
    }

    [Fact]
    public void GetIdentifier_ReturnsAppIdWithPrefix_WhenAppToken()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("appid", "app-123-456")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Equal("[APP:app-123-456]", result);
    }

    [Fact]
    public void GetIdentifier_ReturnsAzpWithPrefix_WhenAppidIsNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("azp", "azp-123-456")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Equal("[APP:azp-123-456]", result);
    }

    [Fact]
    public void GetIdentifier_ReturnsNull_WhenNoIdentifierClaimsPresent()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("custom_claim", "value")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetIdentifier_PrioritizesUserClaimsOverAppClaims()
    {
        // Arrange - both user and app claims present
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "user@example.com"),
            new Claim("appid", "app-123-456")
        };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = InvokeGetIdentifier(claimsPrincipal);

        // Assert - Should prefer user claim
        Assert.Equal("user@example.com", result);
    }

    [Fact]
    public void LoadRequiredItems_ReturnsEmptyList_WhenInputIsEmpty()
    {
        // Act
        var result = InvokeLoadRequiredItems(string.Empty);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void LoadRequiredItems_ReturnsEmptyList_WhenInputIsWhitespace()
    {
        // Act
        var result = InvokeLoadRequiredItems("   ");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void LoadRequiredItems_ParsesCommaSeparatedItems()
    {
        // Act
        var result = InvokeLoadRequiredItems("scope1,scope2,scope3");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("scope1", result);
        Assert.Contains("scope2", result);
        Assert.Contains("scope3", result);
    }

    [Fact]
    public void LoadRequiredItems_RemovesWhitespace()
    {
        // Act
        var result = InvokeLoadRequiredItems("scope1, scope2 , scope3");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("scope1", result);
        Assert.Contains("scope2", result);
        Assert.Contains("scope3", result);
    }

    [Fact]
    public void LoadRequiredItems_HandlesEmptyEntries()
    {
        // Act
        var result = InvokeLoadRequiredItems("scope1,,scope3");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("scope1", result);
        Assert.Contains("scope3", result);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsValid_And_Username_FromPreferredUsername()
    {
        var config = CreateMockOpenIdConnectConfiguration();
        _configManager.Configuration = config;

        var creds = new SigningCredentials(config.SigningKeys.OfType<SecurityKey>().First(), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _validIssuer,
            audience: _validAudience,
            claims: new[] { new Claim("preferred_username", "user@example.com"), new Claim("scp", "user_impersonation") },
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var result = await _validator.ValidateTokenAsync("Bearer " + jwt);

        Assert.True(result.IsValid);
        Assert.Equal("user@example.com", result.Username);
    }

    private OpenIdConnectConfiguration CreateMockOpenIdConnectConfiguration()
    {
        var config = new OpenIdConnectConfiguration
        {
            Issuer = _validIssuer
        };

        var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-key-that-is-long-enough-for-hmac-256"));
        config.SigningKeys.Add(signingKey);

        return config;
    }

    private bool InvokeIsApplicationToken(ClaimsPrincipal claimsPrincipal)
    {
        var method = typeof(AuthorizationValidator).GetMethod("IsApplicationToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method == null)
            throw new MissingMethodException("IsApplicationToken method not found.");
        var result = method.Invoke(null, new[] { claimsPrincipal });
        if (result == null)
            throw new InvalidOperationException("IsApplicationToken invocation returned null.");
        return (bool)result;
    }

    private bool InvokeValidateDelegatedToken(ClaimsPrincipal claimsPrincipal, List<string> requiredScopes, List<string> requiredRoles)
    {
        var method = typeof(AuthorizationValidator).GetMethod("ValidateDelegatedToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = method?.Invoke(null, new object[] { claimsPrincipal, requiredScopes, requiredRoles });
        if (result == null)
            throw new InvalidOperationException("ValidateDelegatedToken invocation returned null.");
        return (bool)result;
    }

    private bool InvokeValidateAppToken(ClaimsPrincipal claimsPrincipal, List<string> requiredScopes, List<string> requiredRoles)
    {
        var method = typeof(AuthorizationValidator).GetMethod("ValidateAppToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null)
            throw new MissingMethodException("ValidateAppToken method not found.");
        var result = method.Invoke(_validator, new object[] { claimsPrincipal, requiredScopes, requiredRoles });
        if (result == null)
            throw new InvalidOperationException("ValidateAppToken invocation returned null.");
        return (bool)result;
    }

    private string? InvokeGetIdentifier(ClaimsPrincipal claimsPrincipal)
    {
        var method = typeof(AuthorizationValidator).GetMethod("GetIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = method?.Invoke(null, new[] { claimsPrincipal });
        return result as string;
    }

    private List<string> InvokeLoadRequiredItems(string items)
    {
        var method = typeof(AuthorizationValidator).GetMethod("LoadRequiredItems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method == null)
            throw new MissingMethodException("LoadRequiredItems method not found.");
        var result = method.Invoke(null, new[] { items });
        if (result == null)
            return new List<string>();
        return (List<string>)result;
    }

    private class TestConfigurationManager : ConfigurationManager<OpenIdConnectConfiguration>
    {
        public bool ShouldThrowException { get; set; }
        public OpenIdConnectConfiguration Configuration { get; set; }

        public TestConfigurationManager() : base("https://test", new OpenIdConnectConfigurationRetriever())
        {
            Configuration = new OpenIdConnectConfiguration();
        }

        public override async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            if (ShouldThrowException)
            {
                throw new InvalidOperationException("Config error");
            }

            return await Task.FromResult(Configuration);
        }
    }
}
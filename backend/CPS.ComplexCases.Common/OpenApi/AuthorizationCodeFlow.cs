using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi;

public class AuthorizationCodeFlow : OpenApiOAuthSecurityFlows
{
    private const string AuthorisationUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize";
    private const string TokenUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";

    public AuthorizationCodeFlow()
    {
        var tenantId = Environment.GetEnvironmentVariable("TenantId");

        AuthorizationCode = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri(string.Format(AuthorisationUrl, tenantId)),
            TokenUrl = new Uri(string.Format(TokenUrl, tenantId)),
            Scopes =
            {
                { "https://graph.microsoft.com/User.Read", "Read user profile" },
                { "openid", "OpenID Connect" },
                { "profile", "User profile" }
            }
        };
    }
}

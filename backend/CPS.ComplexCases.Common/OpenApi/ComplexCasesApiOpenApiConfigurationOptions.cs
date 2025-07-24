using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi;

public class ComplexCasesApiOpenApiConfigurationOptions : BaseOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Large and Complex Case API Endpoints",
        Description = "HTTP API Endpoints for interaction with Large and Complex Cases.",
        TermsOfService = null,
        Contact = new OpenApiContact()
        {
            Name = string.Empty,
            Email = string.Empty,
            Url = null,
        },
        License = new OpenApiLicense()
        {
            Name = string.Empty,
            Url = null,
        },
    };

    public override IDictionary<string, OpenApiSecurityScheme> SecuritySchemes => new Dictionary<string, OpenApiSecurityScheme>
    {
        {
            "oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
         Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode =
             {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{ClientId}/access_as_user", "Access as user" }
                }
            },
        },
                Description = "oauth2 with Microsoft Entra ID",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            }
        }
    };

    public override OpenApiSecurityRequirement SecurityRequirements => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"api://{ClientId}/access_as_user" }
        }
    };

    private static string TenantId
    {
        get
        {
            var tenantId = Environment.GetEnvironmentVariable("TenantId");
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("TenantId environment variable is not set");
            }
            return tenantId;
        }
    }

    private static string ClientId
    {
        get
        {
            var clientId = Environment.GetEnvironmentVariable("ClientId");
            if (string.IsNullOrEmpty(clientId))
            {
                throw new InvalidOperationException("ClientId environment variable is not set");
            }
            return clientId;
        }
    }
}

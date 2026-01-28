using Microsoft.Extensions.Configuration;

namespace CPS.ComplexCases.API.Integration.Tests.Configuration;

public class IntegrationTestSettings
{
    public EgressSettings Egress { get; set; } = new();
    public DdeiSettings DDEI { get; set; } = new();
    public AzureSettings Azure { get; set; } = new();
    public string? CaseManagementDatastoreConnection { get; set; }

    public static IntegrationTestSettings FromConfiguration(IConfiguration configuration)
    {
        var settings = new IntegrationTestSettings
        {
            CaseManagementDatastoreConnection = configuration.GetConnectionString("CaseManagementDatastoreConnection")
        };

        configuration.GetSection("Egress").Bind(settings.Egress);
        configuration.GetSection("DDEI").Bind(settings.DDEI);
        configuration.GetSection("Azure").Bind(settings.Azure);

        return settings;
    }
}

public class EgressSettings
{
    public string? Url { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? WorkspaceId { get; set; }
    public string? TestDestinationPath { get; set; }
    public string? WorkspaceName { get; set; }
    public string? Email { get; set; }

    public bool IsConfigured => !string.IsNullOrEmpty(Url)
        && !string.IsNullOrEmpty(Username)
        && !string.IsNullOrEmpty(Password)
        && !string.IsNullOrEmpty(WorkspaceId);
}

public class DdeiSettings
{
    public string? BaseUrl { get; set; }
    public string? AccessKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int? TestCaseId { get; set; }
    public string? TestUrn { get; set; }
    public string? TestOperationName { get; set; }
    public string? TestDefendantLastName { get; set; }
    public string? TestCmsAreaCode { get; set; }

    public bool IsConfigured => !string.IsNullOrEmpty(BaseUrl)
        && !string.IsNullOrEmpty(AccessKey);

    public bool IsAuthConfigured => IsConfigured
        && !string.IsNullOrEmpty(Username)
        && !string.IsNullOrEmpty(Password);
}

public class AzureSettings
{
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }

    public bool IsConfigured => !string.IsNullOrEmpty(TenantId)
        && !string.IsNullOrEmpty(ClientId)
        && !string.IsNullOrEmpty(ClientSecret)
        && !string.IsNullOrEmpty(Scope);
}

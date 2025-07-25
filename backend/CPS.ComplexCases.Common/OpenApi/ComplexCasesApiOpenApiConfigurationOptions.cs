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
}

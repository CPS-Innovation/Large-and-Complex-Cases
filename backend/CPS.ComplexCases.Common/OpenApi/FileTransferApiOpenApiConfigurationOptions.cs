using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi;

public class FileTransferApiOpenApiConfigurationOptions : BaseOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "File Transfer API Endpoints",
        Description = "HTTP API Endpoints for Complex Cases file transfer operations.",
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
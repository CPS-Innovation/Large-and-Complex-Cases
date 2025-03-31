using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.OpenApi.Filters;

namespace CPS.ComplexCases.OpenApi;

public class OpenApiConfigurationOptions : IOpenApiConfigurationOptions
{
    public OpenApiInfo Info { get; set; } = new OpenApiInfo
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
    public List<OpenApiServer> Servers { get; set; } = [];
    public OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
    public bool IncludeRequestingHostName { get; set; } = true;
    public bool ForceHttp { get; set; }
    public bool ForceHttps { get; set; }
    public List<IDocumentFilter> DocumentFilters { get; set; } = [new EnumDocumentFilter(), new MultiTypeDocumentFilter(), new DateOnlyDocumentFilter(), new OrderByTagsDocumentFilter()];
}

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.Common.OpenApi.Filters;

namespace CPS.ComplexCases.Common.OpenApi;

public class BaseOpenApiConfigurationOptions : IOpenApiConfigurationOptions
{
    public virtual OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "CPS Complex Cases API",
        Description = "HTTP API Endpoints for Complex Cases system.",
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
    public virtual List<IDocumentFilter> DocumentFilters { get; set; } = [
        new EnumDocumentFilter(),
        new MultiTypeDocumentFilter(),
        new DateOnlyDocumentFilter(),
        new OrderByTagsDocumentFilter()
    ];
    public virtual IDictionary<string, OpenApiSecurityScheme> SecuritySchemes => new Dictionary<string, OpenApiSecurityScheme>();
    public virtual OpenApiSecurityRequirement SecurityRequirements => new OpenApiSecurityRequirement();

}
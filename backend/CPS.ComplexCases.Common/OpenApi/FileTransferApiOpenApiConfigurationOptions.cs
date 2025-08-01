using CPS.ComplexCases.Common.OpenApi.Filters;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi;

public class FileTransferApiOpenApiConfigurationOptions : BaseOpenApiConfigurationOptions
{
    public override List<IDocumentFilter> DocumentFilters { get; set; } = new List<IDocumentFilter>
    {
        new EnumDocumentFilter(),
        new MultiTypeDocumentFilter(),
        new DateOnlyDocumentFilter(),
        new OrderByTagsDocumentFilter(),
        new FunctionKeySecurityDocumentFilter()
    };

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
    public override IDictionary<string, OpenApiSecurityScheme> SecuritySchemes => new Dictionary<string, OpenApiSecurityScheme>
    {
        {
            "FunctionKey", new OpenApiSecurityScheme
            {
                Name = "x-functions-key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Function key required in `x-functions-key` header.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "FunctionKey"
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
                    Id = "FunctionKey"
                }
            },
            Array.Empty<string>()
        }
    };

}
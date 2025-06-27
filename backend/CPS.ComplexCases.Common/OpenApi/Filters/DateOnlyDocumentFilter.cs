using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi.Filters;

public class DateOnlyDocumentFilter : IDocumentFilter
{
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        document.Components.Schemas["dateOnly"] = new()
        {
            Type = "string",
            Format = "date",
        };
    }
}
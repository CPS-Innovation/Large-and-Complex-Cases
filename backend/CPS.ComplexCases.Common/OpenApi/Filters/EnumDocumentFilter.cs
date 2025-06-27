using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi.Filters;

public class EnumDocumentFilter : IDocumentFilter
{
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        foreach (var schema in document.Components.Schemas)
        {
            foreach (var property in schema.Value.Properties)
            {
                if (property.Value.Enum.Any())
                {
                    var schemaType = DocumentFilterHelper.FindType(schema.Key);
                    if (schemaType is null) continue;
                    var propertyType = schemaType.GetProperties().Single(p => string.Equals(p.Name, property.Key, StringComparison.InvariantCultureIgnoreCase)).PropertyType;
                    if (propertyType.IsGenericType)
                    {
                        propertyType = propertyType.GetGenericArguments().FirstOrDefault();
                    }

                    property.Value.Enum = Enum.GetNames(propertyType)
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList();
                    property.Value.Type = "string";
                    property.Value.Default = property.Value.Enum[0];
                    property.Value.Format = null;
                }
            }
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi.Filters;

public class MultiTypeDocumentFilter : IDocumentFilter
{
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        foreach (var schema in document.Components.Schemas)
        {
            var type = DocumentFilterHelper.FindType(schema.Key);
            if (type is null) continue;
            foreach (var property in schema.Value.Properties)
            {
                if (property.Value.Type is "object")
                {
                    var prop = type.GetProperties().SingleOrDefault(p => string.Equals(p.Name, property.Key, StringComparison.InvariantCultureIgnoreCase));
                    var dataTypeAttribute = prop?.GetCustomAttribute<DataTypeAttribute>();
                    if (dataTypeAttribute is null) continue;

                    var customTypes = dataTypeAttribute.CustomDataType?.Split(',') ?? [];
                    if (customTypes.Length == 1)
                    {
                        property.Value.Type = customTypes[0];
                    }
                    else
                    {
                        property.Value.OneOf = customTypes.Select(ct => new OpenApiSchema() { Type = ct }).ToList();
                    }
                }
            }
        }
    }
}
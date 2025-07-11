using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.OpenApi.Filters;

public static class DocumentFilterHelper
{
    public static Type? FindType(string key, bool genericSearch = false)
    {
        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
        {
            var schemaType = item.GetTypes().SingleOrDefault(t => string.Equals(t.Name, key, StringComparison.InvariantCultureIgnoreCase));
            if (schemaType is not null)
            {
                return schemaType;
            }
        }

        if (genericSearch)
        {
            return null;
        }

        var genericKey = key.Contains('_')
               ? $"{key.Split("_")[0]}`{key.Split("_").Length - 1}"
               : key;

        var genericSchemaType = FindType(genericKey, true);
        if (genericSchemaType is not null)
        {
            return genericSchemaType;
        }

        throw new KeyNotFoundException($"Unable to generate swagger. Unable to find class {key} in loaded assemblies.");
    }
}
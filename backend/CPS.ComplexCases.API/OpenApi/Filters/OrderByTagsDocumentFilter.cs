using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.OpenApi.Filters;

public class OrderByTagsDocumentFilter : IDocumentFilter
{
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        // Sort by tag and then by path/endpoint
        var sortedPaths = document.Paths.OrderBy(p => p.Value.Operations.First().Value.Tags[0].Name)
            .ThenBy(p => p.Key)
            .ToList();

        document.Paths = [];
        foreach (var path in sortedPaths)
        {
            document.Paths.Add(path.Key, path.Value);
        }
    }
}

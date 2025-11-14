using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.Attributes;

public sealed class FunctionKeyAuthAttribute : OpenApiSecurityAttribute
{
    public FunctionKeyAuthAttribute()
        : base("function_key", SecuritySchemeType.ApiKey)
    {
        Name = "x-functions-key";
        In = OpenApiSecurityLocationType.Header;
        Description = "The Azure Function API Key.";
    }
}

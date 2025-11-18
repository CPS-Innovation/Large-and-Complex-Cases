using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.Attributes;

public sealed class BearerTokenAuthAttribute : OpenApiSecurityAttribute
{
    public BearerTokenAuthAttribute()
        : base("bearer_token", SecuritySchemeType.ApiKey)
    {
        Name = "Authorization";
        In = OpenApiSecurityLocationType.Header;
        Description = "The Bearer Token Authorization header.";
    }
}
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.Common.Attributes;

public sealed class CmsAuthValuesAuthAttribute : OpenApiSecurityAttribute
{
    public CmsAuthValuesAuthAttribute()
        : base("Cms-Auth-Values", SecuritySchemeType.ApiKey)
    {
        Name = "Cms-Auth-Values";
        In = OpenApiSecurityLocationType.Cookie;
        Description = "The CMS Auth Values. This can be retrieved via the Authenticate API Endpoint.";
    }
}

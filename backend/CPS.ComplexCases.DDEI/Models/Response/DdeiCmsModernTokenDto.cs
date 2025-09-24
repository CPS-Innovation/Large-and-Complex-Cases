using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiCmsModernTokenDto
{
    [JsonPropertyName("cmsModernToken")]
    public string CmsModernToken { get; set; } = string.Empty;
}

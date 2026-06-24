using System.Net;
using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Ontap;

public class GetCifsSessionUserResult
{
    [JsonPropertyName("records")]
    public IEnumerable<GetCifsSessionUserResultDto>? Records { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

public class GetCifsSessionUserResultDto
{
    [JsonPropertyName("user")]
    public string? User { get; set; }
}
using System.Net;
using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Ontap;

public class GetFileLockProtocolResult
{
    [JsonPropertyName("records")]
    public IEnumerable<GetFileLockRecordDto>? Records { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

public class GetFileLockRecordDto
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }
    [JsonPropertyName("type")]
    public string? LockType { get; set; }
    [JsonPropertyName("client_address")]
    public string? ClientAddress { get; set; }
}
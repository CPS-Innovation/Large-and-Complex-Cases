using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class GetFileLockRequestDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}
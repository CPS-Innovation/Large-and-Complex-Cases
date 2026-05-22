using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Dto;

public class CaseNameDto
{
    [JsonPropertyName("caseName")]
    public required string CaseName { get; set; }
    [JsonPropertyName("operationName")]
    public required string OperationName { get; set; }
}
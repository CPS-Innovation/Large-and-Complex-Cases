using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Request;

public class CreateEgressWorkspaceRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("templateId")]
    public required string TemplateId { get; set; }
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
}

using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Args;

public class CreateEgressWorkspaceArg
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("template_id")]
    public required string TemplateId { get; set; }
}

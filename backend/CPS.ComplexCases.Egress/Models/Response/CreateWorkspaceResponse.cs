using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class CreateWorkspaceResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }
}
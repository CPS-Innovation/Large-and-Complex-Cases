using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Args;

public class FindWorkspaceArg
{
  [JsonPropertyName("name")]
  public string? Name { get; set; }
}

using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Args;

public class ListEgressWorkspacesArg : PaginationArg
{
  [JsonPropertyName("name")]
  public string? Name { get; set; }
}

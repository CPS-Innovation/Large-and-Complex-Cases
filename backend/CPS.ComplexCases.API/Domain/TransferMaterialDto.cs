using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain;

public class TransferMaterialDto
{
  [JsonPropertyName("destinationPath")]
  public required string DestinationPath { get; set; }
}
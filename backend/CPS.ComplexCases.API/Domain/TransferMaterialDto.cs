using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain;

public class TransferMaterialDto
{
  [JsonPropertyName("filePaths")]
  public required FilePathDto[] FilePaths { get; set; }
}

public class FilePathDto
{
  [JsonPropertyName("destination")]
  public required string Destination { get; set; }
  [JsonPropertyName("source")]
  public required string Source { get; set; }
}
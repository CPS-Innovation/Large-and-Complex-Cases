using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class ListCaseMaterialResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<ListCaseMaterialDataResponse> Data { get; set; }
  [JsonPropertyName("data_info")]
  public required DataInfoResponse DataInfo { get; set; }
}

public class ListCaseMaterialDataResponse
{
  [JsonPropertyName("id")]
  public required string Id { get; set; }
  [JsonPropertyName("filename")]
  public required string FileName { get; set; }
  [JsonPropertyName("date_updated")]
  public DateTime? DateUpdated { get; set; }
  [JsonPropertyName("is_folder")]
  public bool IsFolder { get; set; }
  [JsonPropertyName("version")]
  public int Version { get; set; }
  [JsonPropertyName("path")]
  public required string Path { get; set; }
  [JsonPropertyName("filesize")]
  public long? FileSize { get; set; }
}

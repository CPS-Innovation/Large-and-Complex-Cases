using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class GetCaseMaterialResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<GetCaseMaterialResponseData> Data { get; set; }
  [JsonPropertyName("data_info")]
  public required DataInfoResponse DataInfo { get; set; }
}

public class GetCaseMaterialResponseData
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
}

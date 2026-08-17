using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Response;

public class ListUserBucketsResponse
{
  [JsonPropertyName("buckets")]
  public required IEnumerable<UserBucketResponse> Buckets { get; set; }
}

public class UserBucketResponse
{
  [JsonPropertyName("id")]
  public required Guid Id { get; set; }
  [JsonPropertyName("name")]
  public required string Name { get; set; }
  [JsonPropertyName("displayName")]
  public required string DisplayName { get; set; }
}

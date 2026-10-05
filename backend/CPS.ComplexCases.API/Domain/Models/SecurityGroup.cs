using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Models;

public class SecurityGroup
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }
    [JsonPropertyName("bucketName")]
    public required string BucketName { get; set; }
    [JsonPropertyName("volumeUuid")]
    public required Guid VolumeUuid { get; set; }
    [JsonPropertyName("entryPrefixes")]
    public List<string>? EntryPrefixes { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // Entry prefixes are navigation hints only - they never grant access. NTFS remains the
    // authoritative control, so a configured prefix is still probed before being offered.
    [JsonIgnore]
    public IReadOnlyList<string> NormalisedEntryPrefixes =>
        (EntryPrefixes ?? [])
            .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
            .Select(prefix => prefix.Trim().TrimStart('/'))
            .Where(prefix => prefix.Length > 0)
            .Select(prefix => prefix.TrimEnd('/') + "/")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

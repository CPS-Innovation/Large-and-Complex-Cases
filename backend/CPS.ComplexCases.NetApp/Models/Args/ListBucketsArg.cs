using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class ListBucketsArg
    {
        [JsonPropertyName("continuationToken")]
        public string? ContinuationToken { get; set; }

        [JsonPropertyName("maxBuckets")]
        public int? MaxBuckets { get; set; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }
    }
}
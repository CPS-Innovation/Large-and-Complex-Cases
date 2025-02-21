using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class FindBucketArg
    {
        [JsonPropertyName("bucketName")]
        public required string BucketName { get; set; }
    }
}
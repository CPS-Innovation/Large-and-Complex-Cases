using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class GetObjectArg
    {
        [JsonPropertyName("bucketName")]
        public required string BucketName { get; set; }

        [JsonPropertyName("objectName")]
        public required string ObjectName { get; set; }
    }
}
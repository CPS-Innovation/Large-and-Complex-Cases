using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Args
{
    public class UploadObjectArg
    {
        [JsonPropertyName("bucketName")]
        public required string BucketName { get; set; }

        [JsonPropertyName("objectName")]
        public required string ObjectKey { get; set; }

        [JsonPropertyName("stream")]
        public required Stream Stream { get; set; }
    }
}
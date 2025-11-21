namespace CPS.ComplexCases.NetApp.Models
{
    public class NetAppOptions
    {
        public required string Url { get; set; }
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string RegionName { get; set; }
        public required string BucketName { get; set; }
        public Guid S3ServiceUuid { get; set; } = Guid.Empty;
        public int SessionDurationSeconds { get; set; } = 1800;
    }
}
namespace CPS.ComplexCases.NetApp.Models
{
    public class NetAppOptions
    {
        public required string Url { get; set; }
        public required string RegionName { get; set; }
        public Guid S3ServiceUuid { get; set; } = Guid.Empty;
        public int SessionDurationSeconds { get; set; } = 1800;
        public string PepperVersion { get; set; } = "v1";

        public string RootCaCert { get; set; } = string.Empty;
        public string IssuingCaCert { get; set; } = string.Empty;
        public string IssuingCaCert2 { get; set; } = string.Empty;
    }
}
namespace CPS.ComplexCases.NetApp.Models
{
    public class NetAppOptions
    {
        public required string Url { get; set; }
        public required string RegionName { get; set; }
        public Guid S3ServiceUuid { get; set; } = Guid.Empty;
        public int SessionDurationSeconds { get; set; } = 14400;
        public string PepperVersion { get; set; } = "v1";
        public string RootCaCert { get; set; } = string.Empty;
        public string IssuingCaCert { get; set; } = string.Empty;
        public string IssuingCaCert2 { get; set; } = string.Empty;
        public int SearchMaxSubstringScanItems { get; set; } = 10000;

        // Both NetApp HTTP clients carry only tiny management/metadata calls (RegisterUser,
        // RegenerateUserKeys, HEAD object, zero-byte folder-marker PUT). Bulk file transfers go
        // through the AWS SDK S3 client, which is configured separately and unaffected by this value.
        public int RequestTimeoutSeconds { get; set; } = 100;

        // After a credential error we force-regenerate the S3 key, but a freshly minted key is not
        // always immediately accepted by the S3 data endpoint. This short settle delay lets the new
        // key propagate before the retry uses it, avoiding an immediate repeat 403.
        public int CredentialPropagationDelaySeconds { get; set; } = 5;
    }
}
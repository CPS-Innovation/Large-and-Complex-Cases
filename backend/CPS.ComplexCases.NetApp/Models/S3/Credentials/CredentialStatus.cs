namespace CPS.ComplexCases.NetApp.Models.S3.Credentials;

public class CredentialStatus
{
    public bool Exists { get; set; }
    public bool IsValid { get; set; }
    public bool NeedsRegeneration { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public double RemainingMinutes { get; set; }
}
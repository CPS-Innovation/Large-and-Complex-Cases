namespace CPS.ComplexCases.NetApp.Models.S3.Credentials;

public class S3CredentialsMetadata
{
    public required string UserPrincipalName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Salt { get; set; }
    public DateTime? LastRotated { get; set; }
    public required string PepperVersion { get; set; }
}
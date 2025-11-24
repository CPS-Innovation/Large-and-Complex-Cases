namespace CPS.ComplexCases.NetApp.Models.S3.Credentials;

public class S3Credentials
{
    public required string EncryptedAccessKey { get; set; }
    public required string EncryptedSecretKey { get; set; }
    public required S3CredentialsMetadata Metadata { get; set; }
}

public class S3CredentialsMetadata
{
    public required string UserPrincipalName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public string? Salt { get; set; }
    public DateTime? LastRotated { get; set; }
    public string? KeyVersion { get; set; }
}
namespace CPS.ComplexCases.NetApp.Models.S3.Credentials;

public class S3CredentialsEncrypted
{
    public required string EncryptedAccessKey { get; set; }
    public required string EncryptedSecretKey { get; set; }
    public required S3CredentialsMetadata Metadata { get; set; }
}
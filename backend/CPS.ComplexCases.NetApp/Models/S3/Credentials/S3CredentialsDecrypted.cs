namespace CPS.ComplexCases.NetApp.Models.S3.Credentials;

public class S3CredentialsDecrypted
{
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required S3CredentialsMetadata Metadata { get; set; }
}
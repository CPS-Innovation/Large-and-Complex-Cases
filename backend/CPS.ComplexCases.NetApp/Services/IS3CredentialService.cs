using CPS.ComplexCases.NetApp.Models.S3.Credentials;

namespace CPS.ComplexCases.NetApp.Services;

public interface IS3CredentialService
{
    Task<S3CredentialsDecrypted> GetCredentialsAsync(string oid, string userName, string bearerToken);
}
using CPS.ComplexCases.NetApp.Models.S3.Credentials;

namespace CPS.ComplexCases.NetApp.Services;

public interface IKeyVaultService
{
    Task StoreCredentialsAsync(string key, S3Credentials credentials);
    Task<S3Credentials?> GetCredentialsAsync(string key);
    Task<CredentialStatus> CheckCredentialStatusAsync(string key);
}
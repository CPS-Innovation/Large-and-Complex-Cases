using CPS.ComplexCases.NetApp.Models.S3.Credentials;

namespace CPS.ComplexCases.NetApp.Services;

public interface IKeyVaultService
{
    Task StoreCredentialsAsync(string key, S3CredentialsEncrypted credentials);
    Task<S3CredentialsEncrypted?> GetCredentialsAsync(string key);
    Task<CredentialStatus> CheckCredentialStatusAsync(string key);
    Task<string> GetPepperAsync(string pepperVersion);
}
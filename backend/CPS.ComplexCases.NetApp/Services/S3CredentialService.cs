using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.NetApp;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.NetApp.Services;

public class S3CredentialService : IS3CredentialService
{
    private readonly IKeyVaultService _keyVaultService;
    private readonly INetAppHttpClient _netAppHttpClient;
    private readonly INetAppArgFactory _netAppArgFactory;
    private readonly ICryptographyService _cryptographyService;
    private readonly NetAppOptions _options;
    private readonly ILogger<S3CredentialService> _logger;

    public S3CredentialService(
        IKeyVaultService keyVaultService,
        INetAppHttpClient netAppClient,
        INetAppArgFactory netAppArgFactory,
        ICryptographyService cryptographyService,
        IOptions<NetAppOptions> options,
        ILogger<S3CredentialService> logger)
    {
        _keyVaultService = keyVaultService;
        _netAppHttpClient = netAppClient;
        _netAppArgFactory = netAppArgFactory;
        _cryptographyService = cryptographyService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<S3CredentialsDecrypted> GetCredentialsAsync(string oid, string userName, string bearerToken)
    {
        _logger.LogInformation("Getting S3 credentials for user {oid}", oid);

        var status = await _keyVaultService.CheckCredentialStatusAsync(oid);

        if (status.Exists && status.IsValid)
        {
            // Credentials exist and are still valid (> 5 min remaining)
            _logger.LogInformation(
                "Found valid credentials for {oid} (expires in {RemainingMinutes:F1} minutes)",
                oid,
                status.RemainingMinutes);

            var encryptedCreds = await _keyVaultService.GetCredentialsAsync(oid);

            if (encryptedCreds == null)
            {
                _logger.LogWarning(
                    "Credentials for {oid} reported as existing but could not be retrieved - regenerating",
                    oid);
                return await RegenerateAndStoreCredentialsAsync(oid, userName, bearerToken);
            }

            return await DecryptCredentialsAsync(encryptedCreds, oid);
        }

        // Need to regenerate - either don't exist or expiring soon
        if (!status.Exists)
        {
            _logger.LogInformation(
                "No credentials found for {oid} - generating new credentials",
                oid);
        }
        else
        {
            _logger.LogInformation(
                "Credentials expiring soon for {oid} ({RemainingMinutes:F1} minutes remaining) - regenerating",
                oid,
                status.RemainingMinutes);
        }

        return await RegenerateAndStoreCredentialsAsync(oid, userName, bearerToken, status.Exists);
    }

    private async Task<S3CredentialsDecrypted> RegenerateAndStoreCredentialsAsync(string oid, string userPrincipalName, string bearerToken, bool isRotation = false)
    {
        try
        {
            _logger.LogInformation("Calling NetApp regenerate-keys API for {UserPrincipalName}", userPrincipalName);
            NetAppUserResponse userResponse = new();

            S3CredentialsDecrypted? existingCredentials = null;
            if (isRotation)
            {
                var encryptedCreds = await _keyVaultService.GetCredentialsAsync(oid);
                if (encryptedCreds != null)
                {
                    existingCredentials = await DecryptCredentialsAsync(encryptedCreds, oid);
                }
            }

            try
            {
                userResponse = await _netAppHttpClient.RegenerateUserKeysAsync(_netAppArgFactory.CreateRegenerateUserKeysArg(userPrincipalName, bearerToken, _options.S3ServiceUuid));
            }
            catch (NetAppNotFoundException)
            {
                userResponse = await _netAppHttpClient.RegisterUserAsync(_netAppArgFactory.CreateRegisterUserArg(userPrincipalName, bearerToken, _options.S3ServiceUuid));
            }

            var accessKey = userResponse.Records.FirstOrDefault()?.AccessKey ?? string.Empty;
            var secretKey = userResponse.Records.FirstOrDefault()?.SecretKey ?? string.Empty;

            var saltBytes = _cryptographyService.CreateSalt();
            var saltBase64 = Convert.ToBase64String(saltBytes);

            var pepper = await _keyVaultService.GetPepperAsync(_options.PepperVersion);

            var encryptedAccessKey = await _cryptographyService.EncryptAsync(accessKey, oid, saltBytes, pepper);
            var encryptedSecretKey = await _cryptographyService.EncryptAsync(secretKey, oid, saltBytes, pepper);

            var now = DateTime.UtcNow;

            var encryptedCredentials = new S3CredentialsEncrypted
            {
                EncryptedAccessKey = encryptedAccessKey,
                EncryptedSecretKey = encryptedSecretKey,
                Metadata = new S3CredentialsMetadata
                {
                    UserPrincipalName = userPrincipalName,
                    Salt = saltBase64,
                    CreatedAt = existingCredentials?.Metadata?.CreatedAt ?? now,
                    LastRotated = isRotation ? now : null,
                    PepperVersion = _options.PepperVersion
                }
            };

            _logger.LogInformation("Storing encrypted credentials in Key Vault for {oid}", oid);
            await _keyVaultService.StoreCredentialsAsync(oid, encryptedCredentials);

            if (isRotation)
            {
                _logger.LogInformation(
                    "Successfully rotated S3 credentials for {oid}",
                    oid);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully generated and stored encrypted S3 credentials for {oid}",
                    oid);
            }

            return new S3CredentialsDecrypted
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                Metadata = encryptedCredentials.Metadata
            };
        }
        catch (NetAppClientException ex)
        {
            _logger.LogError(
                ex,
                "Failed to regenerate credentials from NetApp for {UserPrincipalName}",
                userPrincipalName);
            throw new S3CredentialException(
                $"Failed to generate S3 credentials from NetApp: {ex.Message}",
                ex);
        }
        catch (KeyVaultException ex)
        {
            _logger.LogError(
                ex,
                "Failed to store credentials in Key Vault for {UserPrincipalName}",
                userPrincipalName);
            throw new S3CredentialException(
                $"Failed to store credentials: {ex.Message}",
                ex);
        }
    }

    private async Task<S3CredentialsDecrypted> DecryptCredentialsAsync(S3CredentialsEncrypted encryptedCreds, string oid)
    {
        try
        {
            var pepper = await _keyVaultService.GetPepperAsync(encryptedCreds.Metadata.PepperVersion);

            var accessKey = await _cryptographyService.DecryptAsync(
                encryptedCreds.EncryptedAccessKey,
                oid,
                encryptedCreds.Metadata.Salt,
                pepper);

            var secretKey = await _cryptographyService.DecryptAsync(
                encryptedCreds.EncryptedSecretKey,
                oid,
                encryptedCreds.Metadata.Salt,
                pepper);

            return new S3CredentialsDecrypted
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                Metadata = encryptedCreds.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to decrypt credentials for {oid}",
                oid);
            throw new S3CredentialException(
                $"Failed to decrypt S3 credentials: {ex.Message}",
                ex);
        }
    }
}
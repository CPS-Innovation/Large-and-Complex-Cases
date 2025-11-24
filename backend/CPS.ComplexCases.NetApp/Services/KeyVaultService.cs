using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models.S3.Credentials;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.NetApp.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;
    private const string CredentialSecretPrefix = "s3-creds-";

    public KeyVaultService(string keyVaultUrl, ILogger<KeyVaultService> logger)
    {
        _secretClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()
        );
        _logger = logger;
    }
    public async Task StoreCredentialsAsync(string key, S3Credentials credentials)
    {
        try
        {
            var secretName = GetSecretName(key);
            var secretValue = JsonSerializer.Serialize(credentials);
            var secret = new KeyVaultSecret(secretName, secretValue);

            await _secretClient.SetSecretAsync(secret);

            _logger.LogInformation("Stored S3 credentials for user {key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to store credentials for user {key}",
                key);
            throw new KeyVaultException($"Failed to store credentials: {ex.Message}", ex);
        }
    }

    public async Task<S3Credentials?> GetCredentialsAsync(string key)
    {
        try
        {
            var secretName = GetSecretName(key);
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);

            var credentials = JsonSerializer.Deserialize<S3Credentials>(secret.Value);
            _logger.LogInformation("Retrieved S3 credentials for user {key}", key);
            return credentials;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation(
                "No credentials found for user {key}",
                key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve credentials for user {key}",
                key);
            throw new KeyVaultException($"Failed to retrieve credentials: {ex.Message}", ex);
        }
    }

    public async Task<CredentialStatus> CheckCredentialStatusAsync(string key)
    {
        var credentials = await GetCredentialsAsync(key);

        if (credentials == null)
        {
            return new CredentialStatus
            {
                Exists = false,
                IsValid = false,
                NeedsRegeneration = true
            };
        }

        // Check if credentials are expired (60 minutes TTL from NetApp)
        var expiresAt = credentials.Metadata.CreatedAt.AddMinutes(60);
        var now = DateTime.UtcNow;
        var remainingMinutes = (expiresAt - now).TotalMinutes;

        return new CredentialStatus
        {
            Exists = true,
            IsValid = remainingMinutes > 5, // Consider invalid if < 5 minutes remaining
            NeedsRegeneration = remainingMinutes <= 5,
            CreatedAt = credentials.Metadata.CreatedAt,
            ExpiresAt = expiresAt,
            RemainingMinutes = remainingMinutes
        };
    }

    private string GetSecretName(string key) => $"{CredentialSecretPrefix}{key}";
}
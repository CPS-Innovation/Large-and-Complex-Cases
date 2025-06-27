using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.Common.Extensions;

public static class KeyVaultExtensions
{
    public static IConfigurationBuilder AddKeyVaultIfConfigured(this IConfigurationBuilder builder, IConfiguration configuration, ILogger? logger = null)
    {
        var keyVaultUri = configuration["KeyVaultUri"];

        if (string.IsNullOrEmpty(keyVaultUri))
        {
            logger?.LogInformation("KeyVaultUri not configured, skipping Azure Key Vault integration");
            return builder;
        }

        try
        {
            builder.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());

            logger?.LogInformation("Successfully configured Azure Key Vault integration with URI: {KeyVaultUri}", keyVaultUri);
        }
        catch (Exception ex)
        {
            // Log warning but don't fail the application startup
            // This allows local development to continue without Key Vault
            if (logger != null)
            {
                logger.LogWarning(ex, "Could not connect to Key Vault at {KeyVaultUri}. Application will continue without Key Vault integration", keyVaultUri);
            }
            else
            {
                Console.WriteLine($"Warning: Could not connect to Key Vault at {keyVaultUri}: {ex.Message}");
            }
        }

        return builder;
    }
}
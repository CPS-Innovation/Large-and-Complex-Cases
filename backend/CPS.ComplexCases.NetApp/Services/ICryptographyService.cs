namespace CPS.ComplexCases.NetApp.Services;

/// <summary>
/// Provides encryption/decryption services for S3 credentials using user-specific keys
/// </summary>
public interface ICryptographyService
{
    /// <summary>
    /// Encrypts data using AES-256-GCM with a user-specific key derived from objectId and salt
    /// </summary>
    Task<string> EncryptAsync(string plaintext, string objectId, byte[] salt, string pepper);

    /// <summary>
    /// Decrypts data using AES-256-GCM with a user-specific key derived from objectId and salt
    /// </summary>
    Task<string> DecryptAsync(string encryptedData, string objectId, string saltBase64, string pepper);

    /// <summary>
    /// Generates a new random salt
    /// </summary>
    byte[] CreateSalt();

}
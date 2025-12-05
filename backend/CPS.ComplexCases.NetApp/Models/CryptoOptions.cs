using System.Security.Cryptography;

namespace CPS.ComplexCases.NetApp.Models;

// <summary>
/// Defines the cryptographic parameters used for S3 credential encryption.
/// These values must remain constant for compatibility with existing encrypted data.
// </summary>
public class CryptoOptions
{
    /// <summary>
    /// Size of the random salt used in key derivation (32 bytes)
    /// </summary>
    public int SaltSizeBytes { get; init; } = 32;

    /// <summary>
    /// Size of the derived AES-GCM encryption key (32 bytes = 256 bits)
    /// </summary>
    public int KeySizeBytes { get; init; } = 32;

    /// <summary>
    /// Size of the AES-GCM nonce/IV (12 bytes)
    /// </summary>
    public int NonceSizeBytes { get; init; } = 12;

    /// <summary>
    /// Size of the AES-GCM authentication tag (16 bytes = 128 bits)
    /// </summary>
    public int TagSizeBytes { get; init; } = 16;

    /// <summary>
    /// HKDF info parameter for key derivation context binding
    /// </summary>
    public string HkdfInfo { get; init; } = "S3CredentialEncryption";

    /// <summary>
    /// Hash algorithm used in HKDF key derivation
    /// </summary>
    public HashAlgorithmName HashAlgorithm { get; init; } = HashAlgorithmName.SHA256;

    /// <summary>
    /// Validates that all parameters are within acceptable ranges
    /// </summary>
    public void Validate()
    {
        if (SaltSizeBytes < 16)
            throw new InvalidOperationException("Salt size must be at least 16 bytes");
        if (KeySizeBytes != 32)
            throw new InvalidOperationException("Key size must be 32 bytes for AES-256");
        if (NonceSizeBytes != 12)
            throw new InvalidOperationException("Nonce size must be 12 bytes for AES-GCM");
        if (TagSizeBytes < 12 || TagSizeBytes > 16)
            throw new InvalidOperationException("Tag size must be between 12 and 16 bytes");
    }
}
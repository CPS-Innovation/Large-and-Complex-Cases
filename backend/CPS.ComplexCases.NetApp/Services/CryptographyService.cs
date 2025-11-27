using System.Security.Cryptography;
using System.Text;

namespace CPS.ComplexCases.NetApp.Services;

public class CryptographyService : ICryptographyService
{
    private const int SaltSizeBytes = 32;
    private const int KeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public byte[] CreateSalt()
    {
        var salt = new byte[SaltSizeBytes];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    public Task<string> EncryptAsync(string plaintext, string objectId, byte[] salt, string pepper)
    {
        var key = DeriveKey(pepper, objectId, salt);

        var nonce = new byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(key, TagSizeBytes);

        // Use salt as Additional Authenticated Data (AAD) for integrity
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag, salt);

        // nonce + ciphertext + tag
        var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<string> DecryptAsync(string encryptedData, string objectId, string saltBase64, string pepper)
    {
        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var salt = Convert.FromBase64String(saltBase64);

            if (encryptedBytes.Length < NonceSizeBytes + TagSizeBytes)
                throw new CryptographicException("Invalid encrypted data format");

            var nonce = new byte[NonceSizeBytes];
            var tag = new byte[TagSizeBytes];
            var ciphertext = new byte[encryptedBytes.Length - NonceSizeBytes - TagSizeBytes];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSizeBytes);
            Buffer.BlockCopy(encryptedBytes, NonceSizeBytes, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedBytes, NonceSizeBytes + ciphertext.Length, tag, 0, TagSizeBytes);

            var key = DeriveKey(pepper, objectId, salt);

            var plaintext = new byte[ciphertext.Length];
            using var aesGcm = new AesGcm(key, TagSizeBytes);

            // Use salt as AAD - will throw if AAD doesn't match (integrity check)
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, salt);

            return Task.FromResult(Encoding.UTF8.GetString(plaintext));
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Failed to decrypt credentials. Data may have been tampered with.", ex);
        }
    }

    private static byte[] DeriveKey(string pepper, string objectId, byte[] salt)
    {
        // Input Key Material (IKM): pepper + objectId
        var pepperBytes = Encoding.UTF8.GetBytes(pepper);
        var objectIdBytes = Encoding.UTF8.GetBytes(objectId);

        // Combine pepper and objectId as IKM
        var ikm = new byte[pepperBytes.Length + objectIdBytes.Length];
        Buffer.BlockCopy(pepperBytes, 0, ikm, 0, pepperBytes.Length);
        Buffer.BlockCopy(objectIdBytes, 0, ikm, pepperBytes.Length, objectIdBytes.Length);

        var info = Encoding.UTF8.GetBytes("S3CredentialEncryption");

        var key = new byte[KeySizeBytes];
        HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm,
            key,
            salt,
            info
        );

        return key;
    }
}
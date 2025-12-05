using System.Security.Cryptography;
using System.Text;
using CPS.ComplexCases.NetApp.Models;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.NetApp.Services;

public class CryptographyService : ICryptographyService
{
    private readonly CryptoOptions _cryptoOptions;

    public CryptographyService(IOptions<CryptoOptions> cryptoOptions)
    {
        _cryptoOptions = cryptoOptions.Value;
        _cryptoOptions.Validate();
    }

    public byte[] CreateSalt()
    {
        var salt = new byte[_cryptoOptions.SaltSizeBytes];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    public Task<string> EncryptAsync(string plaintext, string objectId, byte[] salt, string pepper)
    {
        var key = DeriveKey(pepper, objectId, salt);
        var nonce = new byte[_cryptoOptions.NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[_cryptoOptions.TagSizeBytes];

        using var aesGcm = new AesGcm(key, _cryptoOptions.TagSizeBytes);
        // Use salt as Additional Authenticated Data (AAD) for integrity
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag, salt);

        // Ciphertext layout: [ nonce | ciphertext | tag ]
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

            if (encryptedBytes.Length < _cryptoOptions.NonceSizeBytes + _cryptoOptions.TagSizeBytes)
                throw new CryptographicException("Invalid encrypted data format");

            // Parse ciphertext layout: [ nonce | ciphertext | tag ]
            var nonce = new byte[_cryptoOptions.NonceSizeBytes];
            var tag = new byte[_cryptoOptions.TagSizeBytes];
            var ciphertext = new byte[encryptedBytes.Length - _cryptoOptions.NonceSizeBytes - _cryptoOptions.TagSizeBytes];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, _cryptoOptions.NonceSizeBytes);
            Buffer.BlockCopy(encryptedBytes, _cryptoOptions.NonceSizeBytes, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedBytes, _cryptoOptions.NonceSizeBytes + ciphertext.Length, tag, 0, _cryptoOptions.TagSizeBytes);

            var key = DeriveKey(pepper, objectId, salt);
            var plaintext = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(key, _cryptoOptions.TagSizeBytes);
            // Use salt as AAD - will throw if AAD doesn't match (integrity check)
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, salt);

            return Task.FromResult(Encoding.UTF8.GetString(plaintext));
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Failed to decrypt credentials. Data may have been tampered with.", ex);
        }
    }

    private byte[] DeriveKey(string pepper, string objectId, byte[] salt)
    {
        // Input Key Material (IKM): pepper + objectId
        var pepperBytes = Encoding.UTF8.GetBytes(pepper);
        var objectIdBytes = Encoding.UTF8.GetBytes(objectId);

        // Combine pepper and objectId as IKM
        var ikm = new byte[pepperBytes.Length + objectIdBytes.Length];
        Buffer.BlockCopy(pepperBytes, 0, ikm, 0, pepperBytes.Length);
        Buffer.BlockCopy(objectIdBytes, 0, ikm, pepperBytes.Length, objectIdBytes.Length);

        var info = Encoding.UTF8.GetBytes(_cryptoOptions.HkdfInfo);
        var key = new byte[_cryptoOptions.KeySizeBytes];

        HKDF.DeriveKey(
            _cryptoOptions.HashAlgorithm,
            ikm,
            key,
            salt,
            info
        );

        return key;
    }
}
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.Identity.Configuration;

namespace MyAi.Infrastructure.Identity;

/// <summary>AES-256-CBC encryption for user-supplied AI API keys at rest.</summary>
public class ApiKeyEncryptionService : IApiKeyEncryptionService
{
    private readonly byte[] _key;

    public ApiKeyEncryptionService(IOptions<JwtConfiguration> configuration)
    {
        var base64Key = configuration.Value.EncryptionKeyBase64;

        _key = string.IsNullOrWhiteSpace(base64Key)
            ? DeriveFallbackKey()
            : Convert.FromBase64String(base64Key);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "Jwt:EncryptionKeyBase64 must decode to exactly 32 bytes for AES-256.");
        }
    }

    public string Encrypt(string apiKey)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedKey)
    {
        var fullBytes = Convert.FromBase64String(encryptedKey);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipherBytes = new byte[fullBytes.Length - iv.Length];

        Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }

    public bool ValidateKeyFormat(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 20 || apiKey.Length > 300)
        {
            return false;
        }

        // OpenRouter keys start with sk-or-; accept any reasonable token format for other providers.
        return apiKey.StartsWith("sk-or-", StringComparison.OrdinalIgnoreCase)
               || apiKey.StartsWith("sk-", StringComparison.OrdinalIgnoreCase)
               || !apiKey.Contains(' ');
    }

    /// <summary>Local-dev fallback key (documented; not for production use).</summary>
    private static byte[] DeriveFallbackKey()
    {
        return SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("MyAi.Development.Encryption.Key.DO.NOT.USE.IN.PROD"));
    }
}

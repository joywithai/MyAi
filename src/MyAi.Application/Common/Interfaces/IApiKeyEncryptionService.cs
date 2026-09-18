namespace MyAi.Application.Common.Interfaces;

/// <summary>AES-256 encryption for user-supplied AI API keys at rest.</summary>
public interface IApiKeyEncryptionService
{
    string Encrypt(string apiKey);

    string Decrypt(string encryptedKey);

    bool ValidateKeyFormat(string apiKey);
}

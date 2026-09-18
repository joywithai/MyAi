namespace MyAi.Application.Common.Interfaces;

public interface IAiProviderFactory
{
    /// <summary>
    /// Creates an AI provider. When a custom key is supplied it is used; otherwise the
    /// system default provider (from system settings) is created with the system key.
    /// </summary>
    IAiProvider Create(string providerName, string? customApiKey = null);
}

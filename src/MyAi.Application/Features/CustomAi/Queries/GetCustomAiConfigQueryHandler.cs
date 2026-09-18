using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.CustomAi;

namespace MyAi.Application.Features.CustomAi.Queries;

/// <summary>Returns the user's custom AI config. The key itself is never returned — masked only.</summary>
public class GetCustomAiConfigQueryHandler : IRequestHandler<GetCustomAiConfigQuery, CustomAiConfigDto?>
{
    private readonly ICustomAiConfigRepository _customAiConfigRepository;
    private readonly IApiKeyEncryptionService _apiKeyEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomAiConfigQueryHandler(
        ICustomAiConfigRepository customAiConfigRepository,
        IApiKeyEncryptionService apiKeyEncryptionService,
        ICurrentUserService currentUserService)
    {
        _customAiConfigRepository = customAiConfigRepository;
        _apiKeyEncryptionService = apiKeyEncryptionService;
        _currentUserService = currentUserService;
    }

    public async Task<CustomAiConfigDto?> Handle(GetCustomAiConfigQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var config = await _customAiConfigRepository.GetByUserIdAsync(userId, ct);

        if (config is null)
        {
            return null;
        }

        string? masked = null;

        try
        {
            var key = _apiKeyEncryptionService.Decrypt(config.EncryptedApiKey);
            masked = key.Length <= 8 ? "sk-...****" : $"{key[..6]}...{key[^4..]}";
        }
        catch
        {
            masked = "****";
        }

        return new CustomAiConfigDto(
            config.ProviderName,
            config.PreferredModel,
            HasKey: true,
            MaskedKey: masked,
            config.IsActive,
            config.LastVerifiedAt);
    }
}

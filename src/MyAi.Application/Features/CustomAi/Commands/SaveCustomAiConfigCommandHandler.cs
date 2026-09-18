using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.CustomAi;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.CustomAi.Commands;

public class SaveCustomAiConfigCommandHandler : IRequestHandler<SaveCustomAiConfigCommand, CustomAiConfigDto>
{
    private readonly ICustomAiConfigRepository _customAiConfigRepository;
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly IApiKeyEncryptionService _apiKeyEncryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SaveCustomAiConfigCommandHandler(
        ICustomAiConfigRepository customAiConfigRepository,
        IFeatureFlagsRepository featureFlagsRepository,
        IApiKeyEncryptionService apiKeyEncryptionService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _customAiConfigRepository = customAiConfigRepository;
        _featureFlagsRepository = featureFlagsRepository;
        _apiKeyEncryptionService = apiKeyEncryptionService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CustomAiConfigDto> Handle(SaveCustomAiConfigCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var role = Domain.Enums.UserRole.Admin;
        var parsedRole = ParseRole(_currentUserService.GetUserRole());
        role = parsedRole;

        var flags = await _featureFlagsRepository.GetByRoleAsync(role, ct);

        if (flags is null || !flags.CanUseCustomApiKey)
        {
            throw new ForbiddenException("Custom AI API keys require a subscription.");
        }

        if (!_apiKeyEncryptionService.ValidateKeyFormat(command.ApiKey))
        {
            throw new Common.Exceptions.ValidationException(
                "API key format looks invalid. OpenRouter keys start with 'sk-or-'.");
        }

        var encrypted = _apiKeyEncryptionService.Encrypt(command.ApiKey.Trim());
        var existing = await _customAiConfigRepository.GetByUserIdAsync(userId, ct);

        if (existing is null)
        {
            existing = UserCustomAiConfig.Create(
                userId, "openrouter", encrypted, command.PreferredModel);
        }
        else
        {
            existing.UpdateKey(encrypted);
            existing.UpdateModel(command.PreferredModel);
            existing.MarkVerified();
        }

        await _customAiConfigRepository.UpsertAsync(existing, ct);

        return new CustomAiConfigDto(
            existing.ProviderName,
            existing.PreferredModel,
            HasKey: true,
            MaskedKey: Mask(command.ApiKey.Trim()),
            existing.IsActive,
            existing.LastVerifiedAt);
    }

    private static Domain.Enums.UserRole ParseRole(string role) =>
        Enum.TryParse<Domain.Enums.UserRole>(role.Replace("public_user", "PublicUser", StringComparison.OrdinalIgnoreCase), true, out var parsed)
            ? parsed
            : Domain.Enums.UserRole.PublicUser;

    private static string Mask(string key) =>
        key.Length <= 8 ? "sk-...****" : $"{key[..6]}...{key[^4..]}";
}

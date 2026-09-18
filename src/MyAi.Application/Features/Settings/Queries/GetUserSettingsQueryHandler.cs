using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Settings.Queries;

/// <summary>Cache-first settings query (settings:{userId}, TTL 15 min).</summary>
public class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserSettingsQueryHandler(
        ISettingsRepository settingsRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var cacheKey = CacheKeys.Settings(userId);

        var cached = await _cacheService.GetAsync<UserSettingsDto>(cacheKey, ct);

        if (cached is not null)
        {
            return cached;
        }

        var settings = await _settingsRepository.GetByUserIdAsync(userId, ct)
                       ?? UserSettings.CreateDefault(userId);

        var dto = _mapper.Map<UserSettingsDto>(settings);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(CacheKeys.SettingsTtlMinutes), ct);

        return dto;
    }
}

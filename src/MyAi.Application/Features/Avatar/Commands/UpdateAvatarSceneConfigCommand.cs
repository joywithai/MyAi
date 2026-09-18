using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Avatar.Commands;

/// <summary>Admin-only: save the global avatar scene configuration for every user.</summary>
public record UpdateAvatarSceneConfigCommand(AvatarSceneConfigDto Config)
    : IRequest<AvatarSceneConfigDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public class UpdateAvatarSceneConfigCommandHandler : IRequestHandler<UpdateAvatarSceneConfigCommand, AvatarSceneConfigDto>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    private readonly IAuditLogRepository _auditLogRepository;

    private readonly ICacheService _cacheService;

    private readonly ICurrentUserService _currentUserService;

    public UpdateAvatarSceneConfigCommandHandler(
        ISystemSettingRepository systemSettingRepository,
        IAuditLogRepository auditLogRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService)
    {
        _systemSettingRepository = systemSettingRepository;
        _auditLogRepository = auditLogRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
    }

    public async Task<AvatarSceneConfigDto> Handle(UpdateAvatarSceneConfigCommand command, CancellationToken ct)
    {
        var config = command.Config ?? AvatarSceneConfigDto.Default();
        config.Clamp();

        var setting = await _systemSettingRepository.GetByKeyAsync(AvatarSceneConfigDto.SettingKey, ct);

        if (setting is null)
        {
            setting = SystemSetting.Create(
                AvatarSceneConfigDto.SettingKey,
                config.ToJson(),
                "Global avatar scene configuration (JSON) — edited from the admin panel");
            await _systemSettingRepository.AddAsync(setting, ct);
        }
        else
        {
            setting.UpdateValue(config.ToJson(), _currentUserService.GetUserId());
            await _systemSettingRepository.UpdateAsync(setting, ct);
        }

        await _cacheService.RemoveAsync(CacheKeys.SystemSettings(), ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                _currentUserService.GetUserId(),
                "avatar_scene.updated",
                nameof(AvatarSceneConfigDto)),
            ct);

        return config;
    }
}

using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Avatar.Queries;

/// <summary>Global avatar scene configuration (admin-controlled; same for everyone).</summary>
public record GetAvatarSceneConfigQuery : IRequest<AvatarSceneConfigDto>;

public class GetAvatarSceneConfigQueryHandler : IRequestHandler<GetAvatarSceneConfigQuery, AvatarSceneConfigDto>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetAvatarSceneConfigQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<AvatarSceneConfigDto> Handle(GetAvatarSceneConfigQuery query, CancellationToken ct)
    {
        var json = await _systemSettingRepository.GetValueAsync(AvatarSceneConfigDto.SettingKey, ct);
        return AvatarSceneConfigDto.FromJson(json);
    }
}

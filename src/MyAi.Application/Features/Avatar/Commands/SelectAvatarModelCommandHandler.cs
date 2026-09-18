using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Settings;

namespace MyAi.Application.Features.Avatar.Commands;

public class SelectAvatarModelCommandHandler : IRequestHandler<SelectAvatarModelCommand, UserSettingsDto>
{
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SelectAvatarModelCommandHandler(
        IAvatarModelRepository avatarModelRepository,
        IFeatureFlagsRepository featureFlagsRepository,
        ISettingsRepository settingsRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _avatarModelRepository = avatarModelRepository;
        _featureFlagsRepository = featureFlagsRepository;
        _settingsRepository = settingsRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserSettingsDto> Handle(SelectAvatarModelCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());

        var model = await _avatarModelRepository.GetByIdAsync(command.ModelId, ct);

        if (model is null || !model.IsActive)
        {
            throw new NotFoundException(ErrorMessages.AvatarModelNotFound);
        }

        if (!model.IsAccessibleByRole(role))
        {
            throw new ForbiddenException(ErrorMessages.AvatarModelLocked);
        }

        var settings = await _settingsRepository.GetByUserIdAsync(userId, ct)
                       ?? UserSettings.CreateDefault(userId);

        settings.SelectAvatarModel(model.Id);
        await _settingsRepository.UpdateAsync(settings, ct);
        await _cacheService.RemoveAsync(CacheKeys.Settings(userId), ct);

        return _mapper.Map<UserSettingsDto>(settings);
    }
}

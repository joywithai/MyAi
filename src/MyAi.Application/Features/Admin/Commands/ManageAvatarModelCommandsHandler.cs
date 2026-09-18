using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Admin.Commands;

public class CreateAvatarModelCommandHandler : IRequestHandler<CreateAvatarModelCommand, AvatarModelDto>
{
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public CreateAvatarModelCommandHandler(
        IAvatarModelRepository avatarModelRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _avatarModelRepository = avatarModelRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<AvatarModelDto> Handle(CreateAvatarModelCommand command, CancellationToken ct)
    {
        var model = Domain.Entities.AvatarModel.Create(
            command.Name, command.FileUrl, command.ThumbnailUrl, RoleConstants.ToUserRole(command.MinRole));

        model.SetDescription(command.Description);

        if (command.IsDefault)
        {
            await _avatarModelRepository.UnsetAllDefaultsAsync(ct);
            model.MakeDefault();
        }

        await _avatarModelRepository.AddAsync(model, ct);
        await _cacheService.RemoveAsync(CacheKeys.AvatarModelsActive(), ct);

        return _mapper.Map<AvatarModelDto>(model);
    }
}

public class UpdateAvatarModelCommandHandler : IRequestHandler<UpdateAvatarModelCommand, AvatarModelDto>
{
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public UpdateAvatarModelCommandHandler(
        IAvatarModelRepository avatarModelRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _avatarModelRepository = avatarModelRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<AvatarModelDto> Handle(UpdateAvatarModelCommand command, CancellationToken ct)
    {
        var model = await _avatarModelRepository.GetByIdAsync(command.Id, ct)
                    ?? throw new NotFoundException("AvatarModel", command.Id);

        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            model.SetDescription(command.Description);
        }

        if (command.Description is not null)
        {
            model.SetDescription(command.Description);
        }

        if (command.ThumbnailUrl is not null)
        {
            model.SetThumbnail(command.ThumbnailUrl);
        }

        if (!string.IsNullOrWhiteSpace(command.MinRole))
        {
            model.UpdateMinRole(RoleConstants.ToUserRole(command.MinRole));
        }

        if (command.IsDefault == true)
        {
            await _avatarModelRepository.UnsetAllDefaultsAsync(ct);
            model.MakeDefault();
        }
        else if (command.IsDefault == false)
        {
            model.Unset();
        }

        if (command.IsActive == true)
        {
            model.Activate();
        }
        else if (command.IsActive == false)
        {
            model.Deactivate();
        }

        await _avatarModelRepository.UpdateAsync(model, ct);
        await _cacheService.RemoveAsync(CacheKeys.AvatarModelsActive(), ct);

        return _mapper.Map<AvatarModelDto>(model);
    }
}

public class DeactivateAvatarModelCommandHandler : IRequestHandler<DeactivateAvatarModelCommand, bool>
{
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly ICacheService _cacheService;

    public DeactivateAvatarModelCommandHandler(
        IAvatarModelRepository avatarModelRepository,
        ICacheService cacheService)
    {
        _avatarModelRepository = avatarModelRepository;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(DeactivateAvatarModelCommand command, CancellationToken ct)
    {
        var model = await _avatarModelRepository.GetByIdAsync(command.Id, ct)
                    ?? throw new NotFoundException("AvatarModel", command.Id);

        model.Deactivate();
        await _avatarModelRepository.UpdateAsync(model, ct);
        await _cacheService.RemoveAsync(CacheKeys.AvatarModelsActive(), ct);

        return true;
    }
}

using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Avatar.Queries;

/// <summary>Returns avatar models accessible to the current user's role.</summary>
public class GetAvailableAvatarModelsQueryHandler
    : IRequestHandler<GetAvailableAvatarModelsQuery, List<AvatarModelDto>>
{
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAvailableAvatarModelsQueryHandler(
        IAvatarModelRepository avatarModelRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _avatarModelRepository = avatarModelRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<AvatarModelDto>> Handle(GetAvailableAvatarModelsQuery query, CancellationToken ct)
    {
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var models = await _avatarModelRepository.GetActiveAsync(ct);

        return models
            .Where(m => m.IsAccessibleByRole(role))
            .Select(_mapper.Map<AvatarModelDto>)
            .ToList();
    }
}

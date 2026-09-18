using AutoMapper;
using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public record GetAllAvatarModelsQuery : IRequest<List<AvatarModelDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public class GetAllAvatarModelsQueryHandler : IRequestHandler<GetAllAvatarModelsQuery, List<AvatarModelDto>>
{
    private readonly IAvatarModelRepository _avatarModelRepository;

    private readonly IMapper _mapper;

    public GetAllAvatarModelsQueryHandler(IAvatarModelRepository avatarModelRepository, IMapper mapper)
    {
        _avatarModelRepository = avatarModelRepository;
        _mapper = mapper;
    }

    public async Task<List<AvatarModelDto>> Handle(GetAllAvatarModelsQuery query, CancellationToken ct) =>
        (await _avatarModelRepository.GetAllAsync(ct)).Select(_mapper.Map<AvatarModelDto>).ToList();
}

public record GetAllExpressionsQuery : IRequest<List<ExpressionDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public class GetAllExpressionsQueryHandler : IRequestHandler<GetAllExpressionsQuery, List<ExpressionDto>>
{
    private readonly IExpressionRepository _expressionRepository;

    private readonly IMapper _mapper;

    public GetAllExpressionsQueryHandler(IExpressionRepository expressionRepository, IMapper mapper)
    {
        _expressionRepository = expressionRepository;
        _mapper = mapper;
    }

    public async Task<List<ExpressionDto>> Handle(GetAllExpressionsQuery query, CancellationToken ct) =>
        (await _expressionRepository.GetAllActiveAsync(ct)).Select(_mapper.Map<ExpressionDto>).ToList();
}

public record GetAllAnimationsQuery : IRequest<List<AnimationDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public class GetAllAnimationsQueryHandler : IRequestHandler<GetAllAnimationsQuery, List<AnimationDto>>
{
    private readonly IAnimationRepository _animationRepository;

    private readonly IMapper _mapper;

    public GetAllAnimationsQueryHandler(IAnimationRepository animationRepository, IMapper mapper)
    {
        _animationRepository = animationRepository;
        _mapper = mapper;
    }

    public async Task<List<AnimationDto>> Handle(GetAllAnimationsQuery query, CancellationToken ct) =>
        (await _animationRepository.GetAllActiveAsync(ct)).Select(_mapper.Map<AnimationDto>).ToList();
}

using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Avatar.Queries;

public record GetAccessibleExpressionsQuery : IRequest<List<ExpressionDto>>;

public class GetAccessibleExpressionsQueryHandler : IRequestHandler<GetAccessibleExpressionsQuery, List<ExpressionDto>>
{
    private readonly IExpressionRepository _expressionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAccessibleExpressionsQueryHandler(
        IExpressionRepository expressionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _expressionRepository = expressionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<ExpressionDto>> Handle(GetAccessibleExpressionsQuery query, CancellationToken ct)
    {
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var expressions = await _expressionRepository.GetAccessibleByRoleAsync(role, ct);

        return expressions.Select(_mapper.Map<ExpressionDto>).ToList();
    }
}

public record GetAccessibleAnimationsQuery : IRequest<List<AnimationDto>>;

public class GetAccessibleAnimationsQueryHandler : IRequestHandler<GetAccessibleAnimationsQuery, List<AnimationDto>>
{
    private readonly IAnimationRepository _animationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAccessibleAnimationsQueryHandler(
        IAnimationRepository animationRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _animationRepository = animationRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<AnimationDto>> Handle(GetAccessibleAnimationsQuery query, CancellationToken ct)
    {
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var animations = await _animationRepository.GetAccessibleByRoleAsync(role, ct);

        return animations.Select(_mapper.Map<AnimationDto>).ToList();
    }
}

using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Settings.Queries;

public class GetMyFeatureFlagsQueryHandler : IRequestHandler<GetMyFeatureFlagsQuery, RoleFeatureFlagsDto>
{
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyFeatureFlagsQueryHandler(
        IFeatureFlagsRepository featureFlagsRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _featureFlagsRepository = featureFlagsRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<RoleFeatureFlagsDto> Handle(GetMyFeatureFlagsQuery query, CancellationToken ct)
    {
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var flags = await _featureFlagsRepository.GetByRoleAsync(role, ct)
                    ?? RoleFeatureFlags.CreateDefault(role);

        return _mapper.Map<RoleFeatureFlagsDto>(flags);
    }
}

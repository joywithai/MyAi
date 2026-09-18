using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public class GetRoleFeatureFlagsQueryHandler : IRequestHandler<GetRoleFeatureFlagsQuery, List<RoleFeatureFlagsDto>>
{
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly IMapper _mapper;

    public GetRoleFeatureFlagsQueryHandler(IFeatureFlagsRepository featureFlagsRepository, IMapper mapper)
    {
        _featureFlagsRepository = featureFlagsRepository;
        _mapper = mapper;
    }

    public async Task<List<RoleFeatureFlagsDto>> Handle(GetRoleFeatureFlagsQuery query, CancellationToken ct)
    {
        var flags = await _featureFlagsRepository.GetAllAsync(ct);

        // Ensure all three roles always appear in the admin matrix.
        var roles = new[] { UserRole.Admin, UserRole.Subscriber, UserRole.PublicUser };

        return roles
            .Select(role => flags.FirstOrDefault(f => f.Role == role) ?? RoleFeatureFlags.CreateDefault(role))
            .Select(_mapper.Map<RoleFeatureFlagsDto>)
            .ToList();
    }
}

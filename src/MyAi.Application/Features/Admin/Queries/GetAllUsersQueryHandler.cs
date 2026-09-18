using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Admin;

namespace MyAi.Application.Features.Admin.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedList<AdminUserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AdminUserDto>> Handle(GetAllUsersQuery query, CancellationToken ct)
    {
        var role = string.IsNullOrWhiteSpace(query.Role)
            ? null
            : RoleConstants.ToUserRole(query.Role);

        UserStatus? status = query.Status?.ToLowerInvariant() switch
        {
            "active" => UserStatus.Active,
            "inactive" => UserStatus.Inactive,
            "banned" => UserStatus.Banned,
            _ => null
        };

        var page = await _userRepository.GetPagedAsync(query.Request, role, status, ct);

        return new PaginatedList<AdminUserDto>(
            page.Items.Select(_mapper.Map<AdminUserDto>).ToList(),
            page.TotalCount, page.PageNumber, page.PageSize);
    }
}

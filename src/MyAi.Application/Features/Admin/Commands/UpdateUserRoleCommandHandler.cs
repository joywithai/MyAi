using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, AdminUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateUserRoleCommandHandler(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IEventPublisher eventPublisher,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AdminUserDto> Handle(UpdateUserRoleCommand command, CancellationToken ct)
    {
        if (command.UserId == _currentUserService.GetUserId())
        {
            throw new ValidationException(ErrorMessages.CannotChangeOwnRole);
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, ct)
                   ?? throw new NotFoundException("User", command.UserId);

        var oldRole = RoleConstants.ToRoleString(user.Role);
        var newRole = RoleConstants.ToUserRole(command.Role);

        user.ChangeRole(newRole);
        await _userRepository.UpdateAsync(user, ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                _currentUserService.GetUserId(),
                "user.role_changed",
                "user",
                user.Id,
                new { role = oldRole },
                new { role = RoleConstants.ToRoleString(newRole) },
                _currentUserService.GetIpAddress(),
                _currentUserService.GetUserAgent()),
            ct);

        await _cacheService.RemoveAsync(CacheKeys.Settings(user.Id), ct);
        await _cacheService.RemoveAsync(CacheKeys.Session(user.Id), ct);

        return _mapper.Map<AdminUserDto>(user);
    }
}

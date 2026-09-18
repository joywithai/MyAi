using AutoMapper;
using MediatR;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, AdminUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateUserStatusCommandHandler(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AdminUserDto> Handle(UpdateUserStatusCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct)
                   ?? throw new NotFoundException("User", command.UserId);

        var oldStatus = user.Status;

        switch (command.Status?.ToLowerInvariant())
        {
            case "active":
                user.Activate();
                break;
            case "banned":
                user.Ban();
                break;
            case "inactive":
                user.Deactivate();
                break;
            default:
                throw new ValidationException("Status must be 'active', 'inactive' or 'banned'.");
        }

        await _userRepository.UpdateAsync(user, ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                _currentUserService.GetUserId(),
                "user.status_changed",
                "user",
                user.Id,
                new { status = oldStatus.ToString().ToLowerInvariant() },
                new { status = user.Status.ToString().ToLowerInvariant() },
                _currentUserService.GetIpAddress(),
                _currentUserService.GetUserAgent()),
            ct);

        await _cacheService.RemoveAsync(CacheKeys.Session(user.Id), ct);

        return _mapper.Map<AdminUserDto>(user);
    }
}

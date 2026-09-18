using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public class UpdateRoleFeatureFlagsCommandHandler
    : IRequestHandler<UpdateRoleFeatureFlagsCommand, RoleFeatureFlagsDto>
{
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateRoleFeatureFlagsCommandHandler(
        IFeatureFlagsRepository featureFlagsRepository,
        IAuditLogRepository auditLogRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _featureFlagsRepository = featureFlagsRepository;
        _auditLogRepository = auditLogRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<RoleFeatureFlagsDto> Handle(UpdateRoleFeatureFlagsCommand command, CancellationToken ct)
    {
        var role = RoleConstants.ToUserRole(command.Role);
        var flags = await _featureFlagsRepository.GetByRoleAsync(role, ct)
                    ?? RoleFeatureFlags.CreateDefault(role);

        var oldValues = _mapper.Map<RoleFeatureFlagsDto>(flags);

        flags.Update(
            command.CanUseCustomApiKey,
            command.CanAccessAllExpressions,
            command.CanAccessAllAnimations,
            command.CanSelectAvatarModel,
            command.CanCustomizeVoice,
            command.CanAccessChatHistory,
            command.MaxConversationHistory,
            command.MaxMessagesPerDay);

        await _featureFlagsRepository.UpdateAsync(flags, ct);

        // Cache invalidation → all users of this role get the new limits immediately.
        await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(RoleConstants.ToRoleString(role)), ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                _currentUserService.GetUserId(),
                "feature_flags.updated",
                "role_feature_flags",
                flags.Id,
                oldValues,
                _mapper.Map<RoleFeatureFlagsDto>(flags),
                _currentUserService.GetIpAddress(),
                _currentUserService.GetUserAgent()),
            ct);

        return _mapper.Map<RoleFeatureFlagsDto>(flags);
    }
}

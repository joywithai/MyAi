using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Admin.Commands;

public class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand, SystemSettingDto>
{
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateSystemSettingCommandHandler(
        ISystemSettingRepository systemSettingRepository,
        IAuditLogRepository auditLogRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _systemSettingRepository = systemSettingRepository;
        _auditLogRepository = auditLogRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<SystemSettingDto> Handle(UpdateSystemSettingCommand command, CancellationToken ct)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(command.Key, ct)
                      ?? throw new NotFoundException("SystemSetting", command.Key);

        var oldValue = setting.Value;
        setting.UpdateValue(command.Value, _currentUserService.GetUserId());
        await _systemSettingRepository.UpdateAsync(setting, ct);

        await _cacheService.RemoveAsync(CacheKeys.SystemSettings(), ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                _currentUserService.GetUserId(),
                "system_setting.updated",
                "system_setting",
                setting.Id,
                new { setting.Key, value = oldValue },
                new { setting.Key, setting.Value },
                _currentUserService.GetIpAddress(),
                _currentUserService.GetUserAgent()),
            ct);

        return _mapper.Map<SystemSettingDto>(setting);
    }
}

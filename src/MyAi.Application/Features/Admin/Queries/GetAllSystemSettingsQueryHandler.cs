using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Admin;

namespace MyAi.Application.Features.Admin.Queries;

public class GetAllSystemSettingsQueryHandler : IRequestHandler<GetAllSystemSettingsQuery, List<SystemSettingDto>>
{
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IMapper _mapper;

    public GetAllSystemSettingsQueryHandler(ISystemSettingRepository systemSettingRepository, IMapper mapper)
    {
        _systemSettingRepository = systemSettingRepository;
        _mapper = mapper;
    }

    public async Task<List<SystemSettingDto>> Handle(GetAllSystemSettingsQuery query, CancellationToken ct)
    {
        var settings = await _systemSettingRepository.GetAllAsync(ct);
        return settings.Select(_mapper.Map<SystemSettingDto>).ToList();
    }
}

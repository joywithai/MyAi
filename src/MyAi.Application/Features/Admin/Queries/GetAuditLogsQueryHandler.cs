using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Admin;

namespace MyAi.Application.Features.Admin.Queries;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(GetAuditLogsQuery query, CancellationToken ct)
    {
        var page = await _auditLogRepository.GetPagedAsync(query.Request, ct);

        return new PaginatedList<AuditLogDto>(
            page.Items.Select(_mapper.Map<AuditLogDto>).ToList(),
            page.TotalCount, page.PageNumber, page.PageSize);
    }
}

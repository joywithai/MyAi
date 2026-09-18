using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);

    public async Task<List<SystemSetting>> GetAllAsync(CancellationToken ct = default) =>
        await _context.SystemSettings.AsNoTracking().OrderBy(s => s.Key).ToListAsync(ct);

    public async Task UpdateAsync(SystemSetting setting, CancellationToken ct = default)
    {
        _context.SystemSettings.Update(setting);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken ct = default) =>
        (await GetByKeyAsync(key, ct))?.Value;
}

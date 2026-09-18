using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task AddAsync(UserSettings settings, CancellationToken ct = default)
    {
        await _context.UserSettings.AddAsync(settings, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UserSettings settings, CancellationToken ct = default)
    {
        _context.UserSettings.Update(settings);
        await _context.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class CustomAiConfigRepository : BaseRepository<UserCustomAiConfig>, ICustomAiConfigRepository
{
    public CustomAiConfigRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserCustomAiConfig?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task UpsertAsync(UserCustomAiConfig config, CancellationToken ct = default)
    {
        var tracked = await DbSet.FirstOrDefaultAsync(c => c.UserId == config.UserId, ct);

        if (tracked is null)
        {
            await DbSet.AddAsync(config, ct);
        }
        else
        {
            tracked.UpdateKey(config.EncryptedApiKey);
            tracked.UpdateModel(config.PreferredModel);
            tracked.MarkVerified();
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        await DbSet
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}

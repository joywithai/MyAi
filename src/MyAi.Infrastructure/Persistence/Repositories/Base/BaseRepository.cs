using Microsoft.EntityFrameworkCore;
using MyAi.Domain.Entities;
using MyAi.Domain.Interfaces;

namespace MyAi.Infrastructure.Persistence.Repositories.Base;

/// <summary>Generic repository with the common CRUD operations.</summary>
public class BaseRepository<T> where T : Entity
{
    protected readonly ApplicationDbContext Context;

    protected readonly DbSet<T> DbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.AnyAsync(e => e.Id == id, ct);

    /// <summary>Soft delete when the entity supports it; hard delete otherwise.</summary>
    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (entity is null)
        {
            return;
        }

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }

        await Context.SaveChangesAsync(ct);
    }
}

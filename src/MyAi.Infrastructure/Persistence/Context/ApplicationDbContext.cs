using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Configurations;

namespace MyAi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IServiceProvider serviceProvider)
        : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    public DbSet<AvatarModel> AvatarModels => Set<AvatarModel>();

    public DbSet<Expression> Expressions => Set<Expression>();

    public DbSet<Animation> Animations => Set<Animation>();

    public DbSet<UserCustomAiConfig> UserCustomAiConfigs => Set<UserCustomAiConfig>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<RoleFeatureFlags> RoleFeatureFlags => Set<RoleFeatureFlags>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists changes, then dispatches domain events raised by entities
    /// (resolved in a fresh scope to avoid circular dependencies).
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        var publisher = _serviceProvider.GetService(typeof(IEventPublisher)) as IEventPublisher;

        if (publisher is null)
        {
            return;
        }

        foreach (var domainEvent in domainEvents)
        {
            await DomainEventDispatcher.DispatchAsync(publisher, domainEvent, cancellationToken);
        }
    }
}

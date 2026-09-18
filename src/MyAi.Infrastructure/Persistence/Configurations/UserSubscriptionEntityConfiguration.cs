using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class UserSubscriptionEntityConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("user_subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(s => s.UserId, "idx_user_subscriptions_user_id");

        builder.Property(s => s.PlanId).HasColumnName("plan_id").IsRequired();

        builder.Property(s => s.Status)
            .HasMaxLength(20)
            .HasConversion(
                s => s switch
                {
                    SubscriptionStatus.Active => "active",
                    SubscriptionStatus.Expired => "expired",
                    _ => "cancelled"
                },
                v => v switch
                {
                    "expired" => SubscriptionStatus.Expired,
                    "cancelled" => SubscriptionStatus.Cancelled,
                    _ => SubscriptionStatus.Active
                })
            .IsRequired();
        builder.HasIndex(s => s.Status, "idx_user_subscriptions_status");

        builder.Property(s => s.StartedAt).HasColumnName("started_at");
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        builder.HasIndex(s => s.ExpiresAt, "idx_user_subscriptions_expires");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SubscriptionPlan>()
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.DomainEvents);
    }
}

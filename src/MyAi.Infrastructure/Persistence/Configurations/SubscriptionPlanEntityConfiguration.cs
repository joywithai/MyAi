using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanEntityConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();

        builder.Property(p => p.RoleGranted)
            .HasColumnName("role_granted")
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLowerInvariant(),
                v => UserEntityConfiguration.ParseRole(v))
            .IsRequired();

        builder.Property(p => p.BillingCycle)
            .HasColumnName("billing_cycle")
            .HasMaxLength(20)
            .HasConversion(
                c => c == Domain.Enums.BillingCycle.Yearly ? "yearly" : "monthly",
                v => v == "yearly" ? Domain.Enums.BillingCycle.Yearly : Domain.Enums.BillingCycle.Monthly)
            .IsRequired();

        builder.Property(p => p.PriceAmount).HasColumnName("price_amount").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(p => p.PriceCurrency).HasColumnName("price_currency").HasMaxLength(10).IsRequired();
        builder.Property(p => p.Description);

        builder.Property(p => p.Features)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .IsRequired();

        builder.Property(p => p.IsActive).HasColumnName("is_active");
        builder.Property(p => p.SortOrder).HasColumnName("sort_order");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(p => p.DomainEvents);
    }
}

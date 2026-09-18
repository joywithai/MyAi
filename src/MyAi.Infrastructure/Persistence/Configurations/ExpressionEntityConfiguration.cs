using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class ExpressionEntityConfiguration : IEntityTypeConfiguration<Expression>
{
    public void Configure(EntityTypeBuilder<Expression> builder)
    {
        builder.ToTable("expressions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.Name, "idx_expressions_name").IsUnique();

        builder.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description);

        builder.Property(e => e.MinRole)
            .HasColumnName("min_role")
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLowerInvariant(),
                v => UserEntityConfiguration.ParseRole(v))
            .IsRequired();

        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.SortOrder).HasColumnName("sort_order");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.MinRole, "idx_expressions_min_role");
        builder.HasIndex(e => e.IsActive, "idx_expressions_is_active");

        builder.Ignore(e => e.DomainEvents);
    }
}

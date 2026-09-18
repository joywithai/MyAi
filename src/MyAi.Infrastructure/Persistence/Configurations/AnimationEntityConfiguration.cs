using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class AnimationEntityConfiguration : IEntityTypeConfiguration<Animation>
{
    public void Configure(EntityTypeBuilder<Animation> builder)
    {
        builder.ToTable("animations");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(a => a.Name, "idx_animations_name").IsUnique();

        builder.Property(a => a.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description);

        builder.Property(a => a.MinRole)
            .HasColumnName("min_role")
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLowerInvariant(),
                v => UserEntityConfiguration.ParseRole(v))
            .IsRequired();

        builder.Property(a => a.IsActive).HasColumnName("is_active");
        builder.Property(a => a.SortOrder).HasColumnName("sort_order");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.MinRole, "idx_animations_min_role");
        builder.HasIndex(a => a.IsActive, "idx_animations_is_active");

        builder.Ignore(a => a.DomainEvents);
    }
}

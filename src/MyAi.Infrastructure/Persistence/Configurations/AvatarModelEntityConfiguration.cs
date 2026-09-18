using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class AvatarModelEntityConfiguration : IEntityTypeConfiguration<AvatarModel>
{
    public void Configure(EntityTypeBuilder<AvatarModel> builder)
    {
        builder.ToTable("avatar_models");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description);
        builder.Property(m => m.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.ThumbnailUrl).HasMaxLength(1000);
        builder.Property(m => m.FileSizeBytes).HasColumnName("file_size_bytes");

        builder.Property(m => m.MinRole)
            .HasColumnName("min_role")
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLowerInvariant(),
                v => UserEntityConfiguration.ParseRole(v))
            .IsRequired();

        builder.Property(m => m.IsDefault).HasColumnName("is_default");
        builder.Property(m => m.IsActive).HasColumnName("is_active");
        builder.Property(m => m.SortOrder).HasColumnName("sort_order");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.IsActive, "idx_avatar_models_is_active");
        builder.HasIndex(m => m.MinRole, "idx_avatar_models_min_role");

        builder.Ignore(m => m.DomainEvents);
    }
}

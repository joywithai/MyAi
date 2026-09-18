using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class SystemSettingEntityConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("system_settings");

        // key is the natural primary key.
        builder.HasKey(s => s.Key);

        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Value).IsRequired();
        builder.Property(s => s.Description);
        builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(s => s.Id);
        builder.Ignore(s => s.DomainEvents);
    }
}

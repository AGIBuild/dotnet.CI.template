using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantEntityConfiguration : IEntityTypeConfiguration<PermissionGrantEntity>
{
    public void Configure(EntityTypeBuilder<PermissionGrantEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(entity => entity.Scope)
            .IsRequired();

        builder.Property(entity => entity.IsGranted)
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasMaxLength(256);

        builder.HasIndex(entity => new { entity.Name, entity.Scope, entity.TenantId, entity.UserId })
            .IsUnique();
    }
}

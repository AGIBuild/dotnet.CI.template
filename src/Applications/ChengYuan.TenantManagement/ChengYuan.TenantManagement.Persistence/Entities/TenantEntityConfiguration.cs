using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.TenantManagement;

public sealed class TenantEntityConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(tenant => tenant.NormalizedName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(tenant => tenant.IsActive)
            .IsRequired();

        builder.Property(tenant => tenant.IsDeleted)
            .IsRequired();

        builder.HasIndex(tenant => tenant.NormalizedName)
            .IsUnique();
    }
}

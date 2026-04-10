using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.Identity;

public sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(role => role.NormalizedName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(role => role.IsActive)
            .IsRequired();

        builder.Property(role => role.IsDeleted)
            .IsRequired();

        builder.HasIndex(role => role.NormalizedName)
            .IsUnique();
    }
}

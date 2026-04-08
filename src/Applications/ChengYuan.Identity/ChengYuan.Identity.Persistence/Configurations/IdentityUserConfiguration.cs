using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.Identity;

public sealed class IdentityUserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.IsDeleted)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedUserName)
            .IsUnique();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();
    }
}

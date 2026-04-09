using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.Identity;

public sealed class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

        builder.Property(userRole => userRole.UserId)
            .IsRequired();

        builder.Property(userRole => userRole.RoleId)
            .IsRequired();
    }
}

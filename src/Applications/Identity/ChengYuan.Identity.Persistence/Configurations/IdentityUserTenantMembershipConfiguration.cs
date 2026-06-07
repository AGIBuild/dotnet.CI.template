using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.Identity;

public sealed class IdentityUserTenantMembershipConfiguration : IEntityTypeConfiguration<IdentityUserTenantMembership>
{
    public void Configure(EntityTypeBuilder<IdentityUserTenantMembership> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(membership => new { membership.UserId, membership.TenantId });

        builder.Property(membership => membership.IsActive)
            .IsRequired();

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(membership => membership.TenantId);
    }
}

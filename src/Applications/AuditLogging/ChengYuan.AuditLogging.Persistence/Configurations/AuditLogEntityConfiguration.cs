using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(entity => entity.StartedAtUtc)
            .IsRequired();

        builder.Property(entity => entity.SerializedProperties)
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasMaxLength(256);

        builder.Property(entity => entity.UserName)
            .HasMaxLength(256);

        builder.Property(entity => entity.CorrelationId)
            .HasMaxLength(256);

        builder.HasIndex(entity => entity.StartedAtUtc);
    }
}

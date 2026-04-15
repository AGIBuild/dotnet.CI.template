using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogEntityChangeEntityConfiguration : IEntityTypeConfiguration<AuditLogEntityChangeEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntityChangeEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EntityTypeFullName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(entity => entity.EntityId)
            .HasMaxLength(256);

        builder.Property(entity => entity.ChangeTime)
            .IsRequired();

        builder.Property(entity => entity.SerializedPropertyChanges)
            .IsRequired();

        builder.HasOne<AuditLogEntity>()
            .WithMany(auditLog => auditLog.EntityChanges)
            .HasForeignKey(entityChange => entityChange.AuditLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entity => entity.AuditLogId);
        builder.HasIndex(entity => entity.EntityTypeFullName);
    }
}

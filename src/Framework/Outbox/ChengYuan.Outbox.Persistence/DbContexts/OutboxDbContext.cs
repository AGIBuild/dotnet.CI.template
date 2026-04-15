using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Outbox;

public sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
            builder.Property(e => e.PayloadTypeName).HasMaxLength(512).IsRequired();
            builder.Property(e => e.PayloadContent).IsRequired();
            builder.Property(e => e.CorrelationId).HasMaxLength(128);
            builder.Property(e => e.LastError).HasMaxLength(4096);

            builder.HasIndex(e => new { e.Status, e.CreatedAtUtc })
                   .HasDatabaseName("IX_OutboxMessages_Status_CreatedAtUtc");
        });
    }
}

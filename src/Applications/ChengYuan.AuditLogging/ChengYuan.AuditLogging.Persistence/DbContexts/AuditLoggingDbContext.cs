using Microsoft.EntityFrameworkCore;

namespace ChengYuan.AuditLogging;

public sealed class AuditLoggingDbContext(DbContextOptions<AuditLoggingDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditLogEntityConfiguration());
    }
}

using Microsoft.EntityFrameworkCore;

namespace ChengYuan.TenantManagement;

public sealed class TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options) : DbContext(options)
{
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
    }
}

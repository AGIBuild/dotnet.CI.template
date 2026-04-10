using Microsoft.EntityFrameworkCore;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionManagementDbContext(DbContextOptions<PermissionManagementDbContext> options) : DbContext(options)
{
    public DbSet<PermissionGrantEntity> PermissionGrants => Set<PermissionGrantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new PermissionGrantEntityConfiguration());
    }
}

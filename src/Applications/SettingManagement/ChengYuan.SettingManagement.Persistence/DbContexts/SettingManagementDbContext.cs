using Microsoft.EntityFrameworkCore;

namespace ChengYuan.SettingManagement;

public sealed class SettingManagementDbContext(DbContextOptions<SettingManagementDbContext> options) : DbContext(options)
{
    public DbSet<SettingValueEntity> SettingValues => Set<SettingValueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new SettingValueEntityConfiguration());
    }
}

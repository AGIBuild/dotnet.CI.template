using Microsoft.EntityFrameworkCore;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureManagementDbContext(DbContextOptions<FeatureManagementDbContext> options) : DbContext(options)
{
    public DbSet<FeatureValueEntity> FeatureValues => Set<FeatureValueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new FeatureValueEntityConfiguration());
    }
}

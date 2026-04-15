using Microsoft.EntityFrameworkCore;

namespace ChengYuan.BackgroundJobs;

public sealed class BackgroundJobDbContext(DbContextOptions<BackgroundJobDbContext> options) : DbContext(options)
{
    public DbSet<BackgroundJobInfo> BackgroundJobs => Set<BackgroundJobInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new BackgroundJobInfoConfiguration());
    }
}

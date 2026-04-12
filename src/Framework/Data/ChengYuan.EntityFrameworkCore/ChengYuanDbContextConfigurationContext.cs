using Microsoft.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore;

public class ChengYuanDbContextConfigurationContext(
    IServiceProvider serviceProvider,
    DbContextOptionsBuilder dbContextOptions)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public DbContextOptionsBuilder DbContextOptions { get; } = dbContextOptions
        ?? throw new ArgumentNullException(nameof(dbContextOptions));
}

public sealed class ChengYuanDbContextConfigurationContext<TDbContext>(
    IServiceProvider serviceProvider,
    DbContextOptionsBuilder dbContextOptions)
    : ChengYuanDbContextConfigurationContext(serviceProvider, dbContextOptions)
    where TDbContext : DbContext;

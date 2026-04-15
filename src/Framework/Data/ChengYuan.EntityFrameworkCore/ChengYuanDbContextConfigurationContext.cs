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

    /// <summary>
    /// The resolved connection string for the current DbContext.
    /// Set by <see cref="DbContextOptionsFactory"/> before the configure action is invoked.
    /// Provider extensions (e.g. UseSqlite) should use this value when available.
    /// </summary>
    public string? ConnectionString { get; internal set; }
}

public sealed class ChengYuanDbContextConfigurationContext<TDbContext>(
    IServiceProvider serviceProvider,
    DbContextOptionsBuilder dbContextOptions)
    : ChengYuanDbContextConfigurationContext(serviceProvider, dbContextOptions)
    where TDbContext : DbContext;

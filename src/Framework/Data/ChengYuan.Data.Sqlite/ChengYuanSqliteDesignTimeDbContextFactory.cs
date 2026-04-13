using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChengYuan.EntityFrameworkCore;

/// <summary>
/// SQLite-specific design-time factory that reads the connection string from
/// <c>appsettings.json</c> located in the WebHost project directory.
/// Each <c>*.Persistence</c> project inherits this with a single line.
/// </summary>
public abstract class ChengYuanSqliteDesignTimeDbContextFactory<TDbContext>
    : ChengYuanDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    protected override void ConfigureProvider(DbContextOptionsBuilder<TDbContext> builder, string connectionString)
    {
        var tableName = MigrationHistoryTableNameResolver.Resolve<TDbContext>();
        builder.UseSqlite(connectionString, b => b.MigrationsHistoryTable(tableName));
    }

    protected override string ResolveConnectionString()
    {
        var basePath = FindWebHostDirectory();
        if (basePath is null)
        {
            return DefaultConnectionString;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        return configuration.GetConnectionString("Default") ?? DefaultConnectionString;
    }

    private static string? FindWebHostDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Hosts", "WebHost");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}

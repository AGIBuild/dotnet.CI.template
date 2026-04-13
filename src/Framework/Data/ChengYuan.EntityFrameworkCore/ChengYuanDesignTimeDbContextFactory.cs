using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChengYuan.EntityFrameworkCore;

/// <summary>
/// Base class for design-time DbContext factories used by <c>dotnet ef</c> CLI.
/// Subclasses must override <see cref="ConfigureProvider"/> to specify the database provider.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type to create at design time.</typeparam>
public abstract class ChengYuanDesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        var builder = new DbContextOptionsBuilder<TDbContext>();

        ConfigureProvider(builder, connectionString);

        return CreateInstance(builder.Options);
    }

    /// <summary>
    /// Gets the default connection string when no configuration file is found.
    /// </summary>
    protected virtual string DefaultConnectionString => "Data Source=chengyuan-webhost.db";

    /// <summary>
    /// Configures the database provider on the options builder.
    /// Must also call <see cref="MigrationHistoryTableNameResolver.Resolve{TDbContext}"/>
    /// to set the migration history table via the provider-specific builder.
    /// </summary>
    protected abstract void ConfigureProvider(DbContextOptionsBuilder<TDbContext> builder, string connectionString);

    /// <summary>
    /// Creates the DbContext instance. Override if the DbContext has a non-standard constructor.
    /// </summary>
    protected virtual TDbContext CreateInstance(DbContextOptions<TDbContext> options)
    {
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
    }

    /// <summary>
    /// Resolves the connection string. Override to customize resolution logic.
    /// </summary>
    protected virtual string ResolveConnectionString()
    {
        return DefaultConnectionString;
    }
}

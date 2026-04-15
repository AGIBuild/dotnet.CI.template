using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanDbContextConfigurationContextSqliteExtensions
{
    public static DbContextOptionsBuilder UseSqlite(
        this ChengYuanDbContextConfigurationContext context,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var effectiveConnectionString = context.ConnectionString ?? connectionString;

        return context.DbContextOptions.UseSqlite(effectiveConnectionString, builder =>
        {
            ApplyMigrationHistoryTable(context, builder);
            sqliteOptionsAction?.Invoke(builder);
        });
    }

    public static DbContextOptionsBuilder UseSqlite(
        this ChengYuanDbContextConfigurationContext context,
        System.Data.Common.DbConnection connection,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(connection);

        return context.DbContextOptions.UseSqlite(connection, builder =>
        {
            ApplyMigrationHistoryTable(context, builder);
            sqliteOptionsAction?.Invoke(builder);
        });
    }

    private static void ApplyMigrationHistoryTable(
        ChengYuanDbContextConfigurationContext context,
        SqliteDbContextOptionsBuilder builder)
    {
        var contextType = context.DbContextOptions.Options.ContextType;
        if (contextType is not null)
        {
            builder.MigrationsHistoryTable(MigrationHistoryTableNameResolver.Resolve(contextType));
        }
    }
}

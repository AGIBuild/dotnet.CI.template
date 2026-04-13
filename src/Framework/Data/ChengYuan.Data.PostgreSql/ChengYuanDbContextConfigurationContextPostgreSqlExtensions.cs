using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanDbContextConfigurationContextPostgreSqlExtensions
{
    public static DbContextOptionsBuilder UseNpgsql(
        this ChengYuanDbContextConfigurationContext context,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return context.DbContextOptions.UseNpgsql(connectionString, optionsBuilder =>
        {
            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            ApplyMigrationHistoryTable(context, optionsBuilder);
            npgsqlOptionsAction?.Invoke(optionsBuilder);
        });
    }

    public static DbContextOptionsBuilder UseNpgsql(
        this ChengYuanDbContextConfigurationContext context,
        System.Data.Common.DbConnection connection,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(connection);

        return context.DbContextOptions.UseNpgsql(connection, optionsBuilder =>
        {
            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            ApplyMigrationHistoryTable(context, optionsBuilder);
            npgsqlOptionsAction?.Invoke(optionsBuilder);
        });
    }

    private static void ApplyMigrationHistoryTable(
        ChengYuanDbContextConfigurationContext context,
        NpgsqlDbContextOptionsBuilder builder)
    {
        var contextType = context.DbContextOptions.Options.ContextType;
        if (contextType is not null)
        {
            builder.MigrationsHistoryTable(MigrationHistoryTableNameResolver.Resolve(contextType));
        }
    }
}

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

        return context.DbContextOptions.UseSqlite(connectionString, sqliteOptionsAction);
    }

    public static DbContextOptionsBuilder UseSqlite(
        this ChengYuanDbContextConfigurationContext context,
        System.Data.Common.DbConnection connection,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(connection);

        return context.DbContextOptions.UseSqlite(connection, sqliteOptionsAction);
    }
}

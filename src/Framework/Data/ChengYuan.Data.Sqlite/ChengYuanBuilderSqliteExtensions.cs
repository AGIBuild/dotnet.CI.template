using ChengYuan.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanBuilderSqliteExtensions
{
    /// <summary>
    /// Configures the ChengYuan framework to use SQLite for all EF Core DbContexts.
    /// </summary>
    public static ChengYuanBuilder UseSqlite(
        this ChengYuanBuilder builder,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.UseSqlite(connectionString, sqliteOptionsAction);
        return builder;
    }
}

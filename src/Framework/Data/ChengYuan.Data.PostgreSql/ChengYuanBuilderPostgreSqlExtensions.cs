using ChengYuan.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanBuilderPostgreSqlExtensions
{
    /// <summary>
    /// Configures the ChengYuan framework to use PostgreSQL (Npgsql) for all EF Core DbContexts.
    /// </summary>
    public static ChengYuanBuilder UseNpgsql(
        this ChengYuanBuilder builder,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.UseNpgsql(connectionString, npgsqlOptionsAction);
        return builder;
    }
}

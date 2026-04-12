using System.Data.Common;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanDbContextOptionsSqliteExtensions
{
    public static IServiceCollection UseSqlite(
        this IServiceCollection services,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCoreSqliteModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context =>
                context.UseSqlite(connectionString, sqliteOptionsAction)));

        return services;
    }

    public static IServiceCollection UseSqlite(
        this IServiceCollection services,
        DbConnection connection,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCoreSqliteModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context =>
                context.UseSqlite(connection, sqliteOptionsAction)));

        return services;
    }

    public static IServiceCollection UseSqlite<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCoreSqliteModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure<TDbContext>(context =>
                context.UseSqlite(connectionString, sqliteOptionsAction)));

        return services;
    }

    public static IServiceCollection UseSqlite<TDbContext>(
        this IServiceCollection services,
        DbConnection connection,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCoreSqliteModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure<TDbContext>(context =>
                context.UseSqlite(connection, sqliteOptionsAction)));

        return services;
    }
}

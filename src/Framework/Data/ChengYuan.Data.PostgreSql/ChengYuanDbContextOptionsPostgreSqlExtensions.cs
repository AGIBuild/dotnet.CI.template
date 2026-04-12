using System.Data.Common;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanDbContextOptionsPostgreSqlExtensions
{
    public static IServiceCollection UseNpgsql(
        this IServiceCollection services,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCorePostgreSqlModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context =>
                context.UseNpgsql(connectionString, npgsqlOptionsAction)));

        return services;
    }

    public static IServiceCollection UseNpgsql(
        this IServiceCollection services,
        DbConnection connection,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCorePostgreSqlModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context =>
                context.UseNpgsql(connection, npgsqlOptionsAction)));

        return services;
    }

    public static IServiceCollection UseNpgsql<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCorePostgreSqlModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure<TDbContext>(context =>
                context.UseNpgsql(connectionString, npgsqlOptionsAction)));

        return services;
    }

    public static IServiceCollection UseNpgsql<TDbContext>(
        this IServiceCollection services,
        DbConnection connection,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddAdditionalModule<ChengYuanEntityFrameworkCorePostgreSqlModule>();
        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure<TDbContext>(context =>
                context.UseNpgsql(connection, npgsqlOptionsAction)));

        return services;
    }
}

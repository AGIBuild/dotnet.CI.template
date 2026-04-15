using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanDbContextOptionsServiceCollectionExtensions
{
    public static IServiceCollection UseDbContextOptions(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context => configureDbContext(context.DbContextOptions)));

        return services;
    }

    public static IServiceCollection UseDbContextOptions(
        this IServiceCollection services,
        Action<Type, DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.Configure<ChengYuanDbContextOptions>(options =>
            options.Configure(context => configureDbContext(
                context.DbContextOptions.Options.ContextType ?? typeof(DbContext),
                context.DbContextOptions)));

        return services;
    }

    public static IServiceCollection AddConfiguredDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<ChengYuanDbContextOptions>(o => o.RegisteredDbContextTypes.Add(typeof(TDbContext)));
        services.AddDbContext<TDbContext>(DbContextOptionsFactory.Configure<TDbContext>);

        return services;
    }

    public static IServiceCollection AddConfiguredDbContextFactory<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<ChengYuanDbContextOptions>(o => o.RegisteredDbContextTypes.Add(typeof(TDbContext)));
        services.AddDbContextFactory<TDbContext>(DbContextOptionsFactory.Configure<TDbContext>);

        return services;
    }

    public static IServiceCollection AddDatabaseMigration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHostedService<DatabaseMigrationHostedService>();

        return services;
    }
}

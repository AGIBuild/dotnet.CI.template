using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using ChengYuan.Core.Entities;
using ChengYuan.Core.Timing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanEntityFrameworkCoreServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkCoreDataAccess<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IDomainEventPublisher, NullDomainEventPublisher>();
        services.AddScoped<IDbContextUnitOfWorkParticipant>(serviceProvider =>
            new DbContextUnitOfWork(serviceProvider.GetRequiredService<TDbContext>()));
        services.TryAddScoped<IUnitOfWork>(serviceProvider =>
            new CompositeDbContextUnitOfWork(
                serviceProvider.GetServices<IDbContextUnitOfWorkParticipant>(),
                serviceProvider.GetRequiredService<IDomainEventPublisher>()));

        return services;
    }

    public static IServiceCollection AddPersistenceModule<TDbContext, TStore, TReader, TImpl>(this IServiceCollection services)
        where TDbContext : DbContext
        where TReader : class
        where TStore : class, TReader
        where TImpl : class, TStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConfiguredDbContextFactory<TDbContext>();
        services.AddEntityFrameworkCoreDataAccess<TDbContext>();
        services.TryAddSingleton<TStore, TImpl>();
        services.TryAddSingleton<TReader>(serviceProvider => serviceProvider.GetRequiredService<TStore>());

        return services;
    }

    public static IServiceCollection AddEfRepository<TDbContext, TEntity, TId>(this IServiceCollection services)
        where TDbContext : DbContext
        where TEntity : class, IAggregateRoot<TId>
        where TId : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IReadOnlyRepository<TEntity, TId>>(CreateRepository<TDbContext, TEntity, TId>);
        services.TryAddScoped<IRepository<TEntity, TId>>(CreateRepository<TDbContext, TEntity, TId>);

        return services;
    }

    private static EfRepository<TDbContext, TEntity, TId> CreateRepository<TDbContext, TEntity, TId>(IServiceProvider serviceProvider)
        where TDbContext : DbContext
        where TEntity : class, IAggregateRoot<TId>
        where TId : notnull
    {
        return new EfRepository<TDbContext, TEntity, TId>(
            serviceProvider.GetRequiredService<TDbContext>(),
            serviceProvider.GetService<IDataFilter<SoftDeleteFilter>>(),
            serviceProvider.GetService<IDataFilter<MultiTenantFilter>>(),
            serviceProvider.GetService<IDataTenantProvider>(),
            serviceProvider.GetService<IClock>(),
            serviceProvider.GetService<IAuditUserProvider>());
    }
}

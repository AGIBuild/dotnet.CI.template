using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;
using ChengYuan.Core.Timing;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Core.EntityFrameworkCore;

public class EfRepository<TDbContext, TEntity, TId>(
    TDbContext dbContext,
    IDataFilter<SoftDeleteFilter>? softDeleteFilter = null,
    IDataFilter<MultiTenantFilter>? multiTenantFilter = null,
    IDataTenantProvider? dataTenantProvider = null,
    IClock? clock = null) : IRepository<TEntity, TId>
    where TDbContext : DbContext
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    protected TDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    protected IQueryable<TEntity> Query => ApplyMultiTenantFilter(ApplySoftDeleteFilter(Set));

    public async ValueTask<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await Query.SingleOrDefaultAsync(entity => entity.Id!.Equals(id), cancellationToken);
    }

    public async ValueTask<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);

        return entity ?? throw new InvalidOperationException(
            $"Aggregate root '{typeof(TEntity).FullName}' with id '{id}' was not found.");
    }

    public async ValueTask<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Set.AddAsync(entity, cancellationToken);
        return entity;
    }

    public ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            var entry = DbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                Set.Attach(entity);
                entry = DbContext.Entry(entity);
            }

            entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;

            if (typeof(IHasDeletionTime).IsAssignableFrom(typeof(TEntity)))
            {
                entry.Property(nameof(IHasDeletionTime.DeletionTime)).CurrentValue = clock?.UtcNow ?? DateTimeOffset.UtcNow;
            }

            entry.State = EntityState.Modified;
            return ValueTask.CompletedTask;
        }

        Set.Remove(entity);
        return ValueTask.CompletedTask;
    }

    private static bool SupportsSoftDelete()
    {
        return typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
    }

    private static bool SupportsMultiTenancy()
    {
        return typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity));
    }

    private IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
    {
        if (!SupportsSoftDelete() || softDeleteFilter?.IsEnabled == false)
        {
            return query;
        }

        return query.Where(entity => !EF.Property<bool>(entity, nameof(ISoftDelete.IsDeleted)));
    }

    private IQueryable<TEntity> ApplyMultiTenantFilter(IQueryable<TEntity> query)
    {
        if (!SupportsMultiTenancy() || multiTenantFilter?.IsEnabled == false || dataTenantProvider?.IsAvailable != true)
        {
            return query;
        }

        var currentTenantId = dataTenantProvider.TenantId;
        return query.Where(entity => EF.Property<Guid?>(entity, nameof(IMultiTenant.TenantId)) == currentTenantId);
    }
}

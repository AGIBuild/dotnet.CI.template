using ChengYuan.Features;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.FeatureManagement;

public sealed class EfFeatureValueStore(IDbContextFactory<FeatureManagementDbContext> dbContextFactory) : IFeatureValueStore
{
    public async ValueTask<FeatureValueRecord?> FindAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.FeatureValues
            .SingleOrDefaultAsync(
                featureValue => featureValue.Name == name
                    && featureValue.Scope == scope
                    && featureValue.TenantId == tenantId
                    && featureValue.UserId == userId,
                cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.FeatureValues
            .ToArrayAsync(cancellationToken);

        return entities
            .OrderBy(featureValue => featureValue.Scope)
            .ThenBy(featureValue => featureValue.Name, StringComparer.Ordinal)
            .ThenBy(featureValue => featureValue.TenantId)
            .ThenBy(featureValue => featureValue.UserId, StringComparer.Ordinal)
            .Select(MapToRecord)
            .ToArray();
    }

    public async ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.FeatureValues
            .SingleOrDefaultAsync(
                featureValue => featureValue.Name == record.Name
                    && featureValue.Scope == record.Scope
                    && featureValue.TenantId == record.TenantId
                    && featureValue.UserId == record.UserId,
                cancellationToken);

        if (entity is null)
        {
            await dbContext.FeatureValues.AddAsync(
                new FeatureValueEntity(Guid.NewGuid(), record.Name, record.Scope, record.Value, record.TenantId, record.UserId),
                cancellationToken);
        }
        else
        {
            entity.Update(record.Name, record.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.FeatureValues
            .SingleOrDefaultAsync(
                featureValue => featureValue.Name == name
                    && featureValue.Scope == scope
                    && featureValue.TenantId == tenantId
                    && featureValue.UserId == userId,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.FeatureValues.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static FeatureValueRecord MapToRecord(FeatureValueEntity entity)
    {
        return new FeatureValueRecord(entity.Name, entity.Scope, entity.ReadValue(), entity.TenantId, entity.UserId);
    }

    private static void ValidateArguments(string name, FeatureScope scope, Guid? tenantId, string? userId)
    {
        _ = new FeatureValueRecord(name, scope, null, tenantId, userId);
    }
}

using ChengYuan.Features;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureValueStore(FeatureManagementDbContext dbContext) : IFeatureValueStore
{
    public async ValueTask<FeatureValueRecord?> FindAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

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
        var entities = await dbContext.FeatureValues
            .OrderBy(featureValue => featureValue.Scope)
            .ThenBy(featureValue => featureValue.Name)
            .ThenBy(featureValue => featureValue.TenantId)
            .ThenBy(featureValue => featureValue.UserId)
            .ToArrayAsync(cancellationToken);

        return entities
            .Select(MapToRecord)
            .ToArray();
    }

    public async ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

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

    }

    public async ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

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
    }

    private static FeatureValueRecord MapToRecord(FeatureValueEntity entity)
        => new(entity.Name, entity.Scope, entity.ReadValue(), entity.TenantId, entity.UserId);

    private static void ValidateArguments(string name, FeatureScope scope, Guid? tenantId, string? userId)
    {
        _ = new FeatureValueRecord(name, scope, null, tenantId, userId);
    }
}

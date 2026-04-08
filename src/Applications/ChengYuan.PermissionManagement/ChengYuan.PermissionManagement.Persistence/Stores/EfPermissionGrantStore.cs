using ChengYuan.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.PermissionManagement;

public sealed class EfPermissionGrantStore(IDbContextFactory<PermissionManagementDbContext> dbContextFactory) : IPermissionGrantStore
{
    public async ValueTask<PermissionGrantRecord?> FindAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.PermissionGrants
            .SingleOrDefaultAsync(
                permissionGrant => permissionGrant.Name == name
                    && permissionGrant.Scope == scope
                    && permissionGrant.TenantId == tenantId
                    && permissionGrant.UserId == userId,
                cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async ValueTask<IReadOnlyList<PermissionGrantRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.PermissionGrants
            .ToArrayAsync(cancellationToken);

        return entities
            .OrderBy(permissionGrant => permissionGrant.Scope)
            .ThenBy(permissionGrant => permissionGrant.Name, StringComparer.Ordinal)
            .ThenBy(permissionGrant => permissionGrant.TenantId)
            .ThenBy(permissionGrant => permissionGrant.UserId, StringComparer.Ordinal)
            .Select(MapToRecord)
            .ToArray();
    }

    public async ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.PermissionGrants
            .SingleOrDefaultAsync(
                permissionGrant => permissionGrant.Name == record.Name
                    && permissionGrant.Scope == record.Scope
                    && permissionGrant.TenantId == record.TenantId
                    && permissionGrant.UserId == record.UserId,
                cancellationToken);

        if (entity is null)
        {
            await dbContext.PermissionGrants.AddAsync(
                new PermissionGrantEntity(Guid.NewGuid(), record.Name, record.Scope, record.IsGranted, record.TenantId, record.UserId),
                cancellationToken);
        }
        else
        {
            entity.Update(record.Name, record.IsGranted);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.PermissionGrants
            .SingleOrDefaultAsync(
                permissionGrant => permissionGrant.Name == name
                    && permissionGrant.Scope == scope
                    && permissionGrant.TenantId == tenantId
                    && permissionGrant.UserId == userId,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.PermissionGrants.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static PermissionGrantRecord MapToRecord(PermissionGrantEntity entity)
    {
        return new PermissionGrantRecord(entity.Name, entity.Scope, entity.IsGranted, entity.TenantId, entity.UserId);
    }

    private static void ValidateArguments(string name, PermissionScope scope, Guid? tenantId, string? userId)
    {
        _ = new PermissionGrantRecord(name, scope, false, tenantId, userId);
    }
}

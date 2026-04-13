using ChengYuan.Settings;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.SettingManagement;

public sealed class EfSettingValueStore(IDbContextFactory<SettingManagementDbContext> dbContextFactory) : ISettingValueStore
{
    public async ValueTask<SettingValueRecord?> FindAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.SettingValues
            .SingleOrDefaultAsync(
                settingValue => settingValue.Name == name
                    && settingValue.Scope == scope
                    && settingValue.TenantId == tenantId
                    && settingValue.UserId == userId,
                cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async ValueTask<IReadOnlyList<SettingValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.SettingValues
            .OrderBy(settingValue => settingValue.Scope)
            .ThenBy(settingValue => settingValue.Name)
            .ThenBy(settingValue => settingValue.TenantId)
            .ThenBy(settingValue => settingValue.UserId)
            .ToArrayAsync(cancellationToken);

        return entities
            .Select(MapToRecord)
            .ToArray();
    }

    public async ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.SettingValues
            .SingleOrDefaultAsync(
                settingValue => settingValue.Name == record.Name
                    && settingValue.Scope == record.Scope
                    && settingValue.TenantId == record.TenantId
                    && settingValue.UserId == record.UserId,
                cancellationToken);

        if (entity is null)
        {
            await dbContext.SettingValues.AddAsync(
                new SettingValueEntity(Guid.NewGuid(), record.Name, record.Scope, record.Value, record.TenantId, record.UserId),
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
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(name, scope, tenantId, userId);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.SettingValues
            .SingleOrDefaultAsync(
                settingValue => settingValue.Name == name
                    && settingValue.Scope == scope
                    && settingValue.TenantId == tenantId
                    && settingValue.UserId == userId,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.SettingValues.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SettingValueRecord MapToRecord(SettingValueEntity entity)
    {
        return new SettingValueRecord(entity.Name, entity.Scope, entity.ReadValue(), entity.TenantId, entity.UserId);
    }

    private static void ValidateArguments(string name, SettingScope scope, Guid? tenantId, string? userId)
    {
        _ = new SettingValueRecord(name, scope, null, tenantId, userId);
    }
}

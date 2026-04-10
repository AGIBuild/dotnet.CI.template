using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

internal sealed class SettingStoreTenantValueProvider(ISettingValueStore store) : ISettingValueProvider
{
    public string Name => "SettingStoreTenant";

    public int Order => 220;

    public async ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, SettingScope.Tenant, tenantId, cancellationToken: cancellationToken);
        return record is null ? null : new SettingValue(record.Value, Name);
    }
}

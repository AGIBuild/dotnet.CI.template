using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public sealed class SettingValueManager(ISettingValueStore store) : ISettingValueManager
{
    public ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default)
    {
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}

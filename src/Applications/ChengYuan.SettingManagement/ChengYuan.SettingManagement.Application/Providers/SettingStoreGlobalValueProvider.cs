using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

internal sealed class SettingStoreGlobalValueProvider(ISettingValueStore store) : ISettingValueProvider
{
    public string Name => "SettingStoreGlobal";

    public int Order => 120;

    public async ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var record = await store.FindAsync(definition.Name, SettingScope.Global, cancellationToken: cancellationToken);
        return record is null ? null : new SettingValue(record.Value, Name);
    }
}

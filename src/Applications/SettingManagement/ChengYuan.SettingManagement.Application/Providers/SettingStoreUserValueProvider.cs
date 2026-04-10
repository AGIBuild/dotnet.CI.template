using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

internal sealed class SettingStoreUserValueProvider(ISettingValueStore store) : ISettingValueProvider
{
    public string Name => "SettingStoreUser";

    public int Order => 320;

    public async ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return null;
        }

        var record = await store.FindAsync(definition.Name, SettingScope.User, userId: context.UserId, cancellationToken: cancellationToken);
        return record is null ? null : new SettingValue(record.Value, Name);
    }
}

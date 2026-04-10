using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public interface ISettingValueStore : ISettingValueReader
{
    ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);
}

using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public interface ISettingValueReader
{
    ValueTask<SettingValueRecord?> FindAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<SettingValueRecord>> GetListAsync(CancellationToken cancellationToken = default);
}

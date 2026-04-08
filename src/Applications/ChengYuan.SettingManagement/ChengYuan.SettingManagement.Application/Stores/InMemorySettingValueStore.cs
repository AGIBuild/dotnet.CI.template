using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public sealed class InMemorySettingValueStore : ISettingValueStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, SettingValueRecord> _globalRecords = new(StringComparer.Ordinal);
    private readonly Dictionary<(Guid TenantId, string Name), SettingValueRecord> _tenantRecords = new();
    private readonly Dictionary<(string UserId, string Name), SettingValueRecord> _userRecords = new();

    public ValueTask<SettingValueRecord?> FindAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            return ValueTask.FromResult(
                scope switch
                {
                    SettingScope.Global => _globalRecords.GetValueOrDefault(name),
                    SettingScope.Tenant when tenantId is Guid resolvedTenantId => _tenantRecords.GetValueOrDefault((resolvedTenantId, name)),
                    SettingScope.User when !string.IsNullOrWhiteSpace(userId) => _userRecords.GetValueOrDefault((userId, name)),
                    SettingScope.Tenant or SettingScope.User => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported setting scope.")
                });
        }
    }

    public ValueTask<IReadOnlyList<SettingValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var records = _globalRecords.Values
                .Concat(_tenantRecords.Values)
                .Concat(_userRecords.Values)
                .OrderBy(record => record.Scope)
                .ThenBy(record => record.Name, StringComparer.Ordinal)
                .ThenBy(record => record.TenantId)
                .ThenBy(record => record.UserId, StringComparer.Ordinal)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<SettingValueRecord>>(records);
        }
    }

    public ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_sync)
        {
            switch (record.Scope)
            {
                case SettingScope.Global:
                    _globalRecords[record.Name] = record;
                    break;

                case SettingScope.Tenant:
                    _tenantRecords[(record.TenantId!.Value, record.Name)] = record;
                    break;

                case SettingScope.User:
                    _userRecords[(record.UserId!, record.Name)] = record;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(record), record.Scope, "Unsupported setting scope.");
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            switch (scope)
            {
                case SettingScope.Global:
                    _globalRecords.Remove(name);
                    break;

                case SettingScope.Tenant when tenantId is Guid resolvedTenantId:
                    _tenantRecords.Remove((resolvedTenantId, name));
                    break;

                case SettingScope.User when !string.IsNullOrWhiteSpace(userId):
                    _userRecords.Remove((userId, name));
                    break;

                case SettingScope.Tenant:
                    throw new ArgumentException("Tenant settings must specify a tenant id.", nameof(tenantId));

                case SettingScope.User:
                    throw new ArgumentException("User settings must specify a user id.", nameof(userId));

                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported setting scope.");
            }
        }

        return ValueTask.CompletedTask;
    }
}

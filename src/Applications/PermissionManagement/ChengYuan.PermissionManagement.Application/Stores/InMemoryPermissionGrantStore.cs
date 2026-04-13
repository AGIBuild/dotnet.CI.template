using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

public sealed class InMemoryPermissionGrantStore : IPermissionGrantStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, PermissionGrantRecord> _globalRecords = new(StringComparer.Ordinal);
    private readonly Dictionary<(Guid TenantId, string Name), PermissionGrantRecord> _tenantRecords = new();
    private readonly Dictionary<(string UserId, string Name), PermissionGrantRecord> _userRecords = new();

    public ValueTask<PermissionGrantRecord?> FindAsync(
        string name,
        PermissionScope scope,
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
                    PermissionScope.Global => _globalRecords.GetValueOrDefault(name),
                    PermissionScope.Tenant when tenantId is Guid resolvedTenantId => _tenantRecords.GetValueOrDefault((resolvedTenantId, name)),
                    PermissionScope.User when !string.IsNullOrWhiteSpace(userId) => _userRecords.GetValueOrDefault((userId, name)),
                    PermissionScope.Tenant or PermissionScope.User => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported permission scope.")
                });
        }
    }

    public ValueTask<IReadOnlyList<PermissionGrantRecord>> GetListAsync(CancellationToken cancellationToken = default)
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

            return ValueTask.FromResult<IReadOnlyList<PermissionGrantRecord>>(records);
        }
    }

    public ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_sync)
        {
            switch (record.Scope)
            {
                case PermissionScope.Global:
                    _globalRecords[record.Name] = record;
                    break;

                case PermissionScope.Tenant:
                    _tenantRecords[(record.TenantId!.Value, record.Name)] = record;
                    break;

                case PermissionScope.User:
                    _userRecords[(record.UserId!, record.Name)] = record;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(record), record.Scope, "Unsupported permission scope.");
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            switch (scope)
            {
                case PermissionScope.Global:
                    _globalRecords.Remove(name);
                    break;

                case PermissionScope.Tenant when tenantId is Guid resolvedTenantId:
                    _tenantRecords.Remove((resolvedTenantId, name));
                    break;

                case PermissionScope.User when !string.IsNullOrWhiteSpace(userId):
                    _userRecords.Remove((userId, name));
                    break;

                case PermissionScope.Tenant:
                    throw new ArgumentException("Tenant grants must specify a tenant id.", nameof(tenantId));

                case PermissionScope.User:
                    throw new ArgumentException("User grants must specify a user id.", nameof(userId));

                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported permission scope.");
            }
        }

        return ValueTask.CompletedTask;
    }
}

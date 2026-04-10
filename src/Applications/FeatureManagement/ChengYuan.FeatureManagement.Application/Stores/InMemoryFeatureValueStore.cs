using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

public sealed class InMemoryFeatureValueStore : IFeatureValueStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, FeatureValueRecord> _globalRecords = new(StringComparer.Ordinal);
    private readonly Dictionary<(Guid TenantId, string Name), FeatureValueRecord> _tenantRecords = new();
    private readonly Dictionary<(string UserId, string Name), FeatureValueRecord> _userRecords = new();

    public ValueTask<FeatureValueRecord?> FindAsync(
        string name,
        FeatureScope scope,
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
                    FeatureScope.Global => _globalRecords.GetValueOrDefault(name),
                    FeatureScope.Tenant when tenantId is Guid resolvedTenantId => _tenantRecords.GetValueOrDefault((resolvedTenantId, name)),
                    FeatureScope.User when !string.IsNullOrWhiteSpace(userId) => _userRecords.GetValueOrDefault((userId, name)),
                    FeatureScope.Tenant or FeatureScope.User => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported feature scope.")
                });
        }
    }

    public ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
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

            return ValueTask.FromResult<IReadOnlyList<FeatureValueRecord>>(records);
        }
    }

    public ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_sync)
        {
            switch (record.Scope)
            {
                case FeatureScope.Global:
                    _globalRecords[record.Name] = record;
                    break;

                case FeatureScope.Tenant:
                    _tenantRecords[(record.TenantId!.Value, record.Name)] = record;
                    break;

                case FeatureScope.User:
                    _userRecords[(record.UserId!, record.Name)] = record;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(record), record.Scope, "Unsupported feature scope.");
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            switch (scope)
            {
                case FeatureScope.Global:
                    _globalRecords.Remove(name);
                    break;

                case FeatureScope.Tenant when tenantId is Guid resolvedTenantId:
                    _tenantRecords.Remove((resolvedTenantId, name));
                    break;

                case FeatureScope.User when !string.IsNullOrWhiteSpace(userId):
                    _userRecords.Remove((userId, name));
                    break;

                case FeatureScope.Tenant:
                    throw new ArgumentException("Tenant features must specify a tenant id.", nameof(tenantId));

                case FeatureScope.User:
                    throw new ArgumentException("User features must specify a user id.", nameof(userId));

                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported feature scope.");
            }
        }

        return ValueTask.CompletedTask;
    }
}

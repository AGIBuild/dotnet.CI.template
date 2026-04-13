using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Data;

public sealed class DataSeedContext
{
    private readonly Dictionary<string, object?> _properties = new(StringComparer.Ordinal);

    public DataSeedContext(Guid? tenantId = null)
    {
        TenantId = tenantId;
    }

    public Guid? TenantId { get; }

    public object? this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : null;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            _properties[key] = value;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ChengYuan.Settings;

public sealed class SettingDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

    public SettingDefinition(string name, Type valueType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(valueType);

        Name = name;
        ValueType = valueType;
    }

    public string Name { get; }

    public Type ValueType { get; }

    public object? DefaultValue { get; internal set; }

    public string? DisplayName { get; internal set; }

    public string? Description { get; internal set; }

    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

    public bool TryGetMetadata(string name, out object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _metadata.TryGetValue(name, out value);
    }

    internal void SetMetadata(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _metadata[name] = value;
    }
}

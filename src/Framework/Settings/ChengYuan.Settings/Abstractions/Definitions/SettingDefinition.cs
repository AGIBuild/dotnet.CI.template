using System;
using System.Collections.Generic;

namespace ChengYuan.Settings;

public sealed class SettingDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

    internal SettingDefinition(
        string name,
        Type valueType,
        object? defaultValue = null,
        string? displayName = null,
        SettingGroupDefinition? group = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(valueType);

        Name = name;
        ValueType = valueType;
        DefaultValue = defaultValue;
        DisplayName = displayName;
        Group = group;
    }

    public string Name { get; }

    public Type ValueType { get; }

    public object? DefaultValue { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public SettingGroupDefinition? Group { get; }

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

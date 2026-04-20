using System;
using System.Collections.Generic;

namespace ChengYuan.Features;

public sealed class FeatureDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

    internal FeatureDefinition(
        string name,
        Type valueType,
        object? defaultValue = null,
        string? displayName = null,
        FeatureGroupDefinition? group = null)
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

    public FeatureGroupDefinition? Group { get; }

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

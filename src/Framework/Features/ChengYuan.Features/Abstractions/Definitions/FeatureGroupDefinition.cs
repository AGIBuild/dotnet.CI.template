using System;
using System.Collections.Generic;

namespace ChengYuan.Features;

public sealed class FeatureGroupDefinition
{
    private readonly List<FeatureDefinition> _features = [];

    internal FeatureGroupDefinition(string name, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public IReadOnlyList<FeatureDefinition> Features => _features;

    public FeatureDefinition AddFeature<TValue>(
        string name,
        TValue? defaultValue = default,
        string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = new FeatureDefinition(name, typeof(TValue), defaultValue, displayName, this);
        _features.Add(definition);
        return definition;
    }
}

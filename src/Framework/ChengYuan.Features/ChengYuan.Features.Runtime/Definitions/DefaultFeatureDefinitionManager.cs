using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Features;

internal sealed class DefaultFeatureDefinitionManager : IFeatureDefinitionManager
{
    private readonly Dictionary<string, FeatureDefinition> _definitions = new(StringComparer.Ordinal);

    public FeatureDefinitionBuilder AddOrUpdate<TValue>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = new FeatureDefinition(name, typeof(TValue));
        _definitions[name] = definition;
        return new FeatureDefinitionBuilder(definition);
    }

    public FeatureDefinition? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.GetValueOrDefault(name);
    }

    public FeatureDefinition GetDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Find(name)
            ?? throw new InvalidOperationException($"Feature '{name}' is not defined.");
    }

    public IReadOnlyCollection<FeatureDefinition> GetAll()
    {
        return _definitions.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.ContainsKey(name);
    }
}

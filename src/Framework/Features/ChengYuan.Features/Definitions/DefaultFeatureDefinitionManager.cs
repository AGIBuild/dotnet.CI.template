using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Features;

internal sealed class DefaultFeatureDefinitionManager(
    IEnumerable<IFeatureDefinitionContributor> contributors) : IFeatureDefinitionManager
{
    private readonly object _syncLock = new();
    private readonly Dictionary<string, FeatureGroupDefinition> _groups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FeatureDefinition> _features = new(StringComparer.Ordinal);
    private bool _initialized;

    public IReadOnlyList<FeatureGroupDefinition> GetGroups()
    {
        EnsureInitialized();
        return _groups.Values.ToArray();
    }

    public FeatureGroupDefinition? GetGroupOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _groups.GetValueOrDefault(name);
    }

    public FeatureGroupDefinition GetGroup(string name)
    {
        return GetGroupOrNull(name)
            ?? throw new InvalidOperationException($"Feature group '{name}' is not defined.");
    }

    public FeatureDefinition? GetOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _features.GetValueOrDefault(name);
    }

    public FeatureDefinition GetFeature(string name)
    {
        return GetOrNull(name)
            ?? throw new InvalidOperationException($"Feature '{name}' is not defined.");
    }

    public IReadOnlyCollection<FeatureDefinition> GetAll()
    {
        EnsureInitialized();
        return _features.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _features.ContainsKey(name);
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_syncLock)
        {
            if (_initialized) return;

            var context = new DefinitionContext(this);
            foreach (var contributor in contributors)
            {
                contributor.Define(context);
            }

            foreach (var group in _groups.Values)
            {
                foreach (var feature in group.Features)
                {
                    _features[feature.Name] = feature;
                }
            }

            _initialized = true;
        }
    }

    private sealed class DefinitionContext(DefaultFeatureDefinitionManager manager) : IFeatureDefinitionContext
    {
        public FeatureGroupDefinition AddGroup(string name, string? displayName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.ContainsKey(name))
            {
                throw new InvalidOperationException($"Feature group '{name}' is already defined.");
            }

            var group = new FeatureGroupDefinition(name, displayName);
            manager._groups[name] = group;
            return group;
        }

        public FeatureGroupDefinition? GetGroupOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._groups.GetValueOrDefault(name);
        }

        public FeatureGroupDefinition GetGroup(string name)
        {
            return GetGroupOrNull(name)
                ?? throw new InvalidOperationException($"Feature group '{name}' is not defined.");
        }

        public FeatureDefinition? GetFeatureOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._features.GetValueOrDefault(name);
        }

        public void RemoveGroup(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.Remove(name, out var group))
            {
                foreach (var feature in group.Features)
                {
                    manager._features.Remove(feature.Name);
                }
            }
        }
    }
}

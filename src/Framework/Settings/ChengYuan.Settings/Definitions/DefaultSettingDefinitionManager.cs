using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Settings;

internal sealed class DefaultSettingDefinitionManager(
    IEnumerable<ISettingDefinitionContributor> contributors) : ISettingDefinitionManager
{
    private readonly object _syncLock = new();
    private readonly Dictionary<string, SettingGroupDefinition> _groups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SettingDefinition> _settings = new(StringComparer.Ordinal);
    private bool _initialized;

    public IReadOnlyList<SettingGroupDefinition> GetGroups()
    {
        EnsureInitialized();
        return _groups.Values.ToArray();
    }

    public SettingGroupDefinition? GetGroupOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _groups.GetValueOrDefault(name);
    }

    public SettingGroupDefinition GetGroup(string name)
    {
        return GetGroupOrNull(name)
            ?? throw new InvalidOperationException($"Setting group '{name}' is not defined.");
    }

    public SettingDefinition? GetOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _settings.GetValueOrDefault(name);
    }

    public SettingDefinition GetSetting(string name)
    {
        return GetOrNull(name)
            ?? throw new InvalidOperationException($"Setting '{name}' is not defined.");
    }

    public IReadOnlyCollection<SettingDefinition> GetAll()
    {
        EnsureInitialized();
        return _settings.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _settings.ContainsKey(name);
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
                foreach (var setting in group.Settings)
                {
                    _settings[setting.Name] = setting;
                }
            }

            _initialized = true;
        }
    }

    private sealed class DefinitionContext(DefaultSettingDefinitionManager manager) : ISettingDefinitionContext
    {
        public SettingGroupDefinition AddGroup(string name, string? displayName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.ContainsKey(name))
            {
                throw new InvalidOperationException($"Setting group '{name}' is already defined.");
            }

            var group = new SettingGroupDefinition(name, displayName);
            manager._groups[name] = group;
            return group;
        }

        public SettingGroupDefinition? GetGroupOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._groups.GetValueOrDefault(name);
        }

        public SettingGroupDefinition GetGroup(string name)
        {
            return GetGroupOrNull(name)
                ?? throw new InvalidOperationException($"Setting group '{name}' is not defined.");
        }

        public SettingDefinition? GetSettingOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._settings.GetValueOrDefault(name);
        }

        public void RemoveGroup(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.Remove(name, out var group))
            {
                foreach (var setting in group.Settings)
                {
                    manager._settings.Remove(setting.Name);
                }
            }
        }
    }
}

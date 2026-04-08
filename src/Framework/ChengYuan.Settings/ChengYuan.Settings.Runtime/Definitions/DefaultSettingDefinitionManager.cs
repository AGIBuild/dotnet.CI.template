using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Settings;

internal sealed class DefaultSettingDefinitionManager : ISettingDefinitionManager
{
    private readonly Dictionary<string, SettingDefinition> _definitions = new(StringComparer.Ordinal);

    public SettingDefinitionBuilder AddOrUpdate<TValue>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = new SettingDefinition(name, typeof(TValue));
        _definitions[name] = definition;
        return new SettingDefinitionBuilder(definition);
    }

    public SettingDefinition? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.GetValueOrDefault(name);
    }

    public SettingDefinition GetDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Find(name)
            ?? throw new InvalidOperationException($"Setting '{name}' is not defined.");
    }

    public IReadOnlyCollection<SettingDefinition> GetAll()
    {
        return _definitions.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.ContainsKey(name);
    }
}

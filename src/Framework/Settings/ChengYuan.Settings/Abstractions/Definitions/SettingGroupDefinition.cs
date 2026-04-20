using System;
using System.Collections.Generic;

namespace ChengYuan.Settings;

public sealed class SettingGroupDefinition
{
    private readonly List<SettingDefinition> _settings = [];

    internal SettingGroupDefinition(string name, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public IReadOnlyList<SettingDefinition> Settings => _settings;

    public SettingDefinition AddSetting<TValue>(
        string name,
        TValue? defaultValue = default,
        string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = new SettingDefinition(name, typeof(TValue), defaultValue, displayName, this);
        _settings.Add(definition);
        return definition;
    }
}

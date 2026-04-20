namespace ChengYuan.Settings;

public interface ISettingDefinitionContext
{
    SettingGroupDefinition AddGroup(string name, string? displayName = null);

    SettingGroupDefinition? GetGroupOrNull(string name);

    SettingGroupDefinition GetGroup(string name);

    SettingDefinition? GetSettingOrNull(string name);

    void RemoveGroup(string name);
}

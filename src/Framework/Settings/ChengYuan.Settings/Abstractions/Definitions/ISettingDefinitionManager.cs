using System.Collections.Generic;

namespace ChengYuan.Settings;

public interface ISettingDefinitionManager
{
    IReadOnlyList<SettingGroupDefinition> GetGroups();

    SettingGroupDefinition? GetGroupOrNull(string name);

    SettingGroupDefinition GetGroup(string name);

    SettingDefinition? GetOrNull(string name);

    SettingDefinition GetSetting(string name);

    IReadOnlyCollection<SettingDefinition> GetAll();

    bool IsDefined(string name);
}

using System.Collections.Generic;

namespace ChengYuan.Settings;

public interface ISettingDefinitionManager
{
    SettingDefinitionBuilder AddOrUpdate<TValue>(string name);

    SettingDefinition? Find(string name);

    SettingDefinition GetDefinition(string name);

    IReadOnlyCollection<SettingDefinition> GetAll();

    bool IsDefined(string name);
}

using System.Collections.Generic;

namespace ChengYuan.Authorization;

public interface IPermissionDefinitionManager
{
    PermissionDefinitionBuilder AddOrUpdate(string name);

    PermissionDefinition? Find(string name);

    PermissionDefinition GetDefinition(string name);

    IReadOnlyCollection<PermissionDefinition> GetAll();

    bool IsDefined(string name);
}

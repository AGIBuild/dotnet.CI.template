using System.Collections.Generic;

namespace ChengYuan.Authorization;

public interface IPermissionDefinitionManager
{
    IReadOnlyList<PermissionGroupDefinition> GetGroups();

    PermissionGroupDefinition? GetGroupOrNull(string name);

    PermissionGroupDefinition GetGroup(string name);

    PermissionDefinition? GetOrNull(string name);

    PermissionDefinition GetPermission(string name);

    IReadOnlyCollection<PermissionDefinition> GetAll();

    bool IsDefined(string name);
}

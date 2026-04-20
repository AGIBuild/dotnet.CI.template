namespace ChengYuan.Authorization;

public interface IPermissionDefinitionContext
{
    PermissionGroupDefinition AddGroup(string name, string? displayName = null);

    PermissionGroupDefinition? GetGroupOrNull(string name);

    PermissionGroupDefinition GetGroup(string name);

    PermissionDefinition? GetPermissionOrNull(string name);

    void RemoveGroup(string name);
}

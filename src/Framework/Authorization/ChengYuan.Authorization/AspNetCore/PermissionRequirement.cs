using Microsoft.AspNetCore.Authorization;

namespace ChengYuan.Authorization;

public sealed class PermissionRequirement(string permissionName) : IAuthorizationRequirement
{
    public string PermissionName { get; } = permissionName;
}

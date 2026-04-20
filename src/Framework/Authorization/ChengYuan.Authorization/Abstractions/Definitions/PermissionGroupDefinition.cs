using System;
using System.Collections.Generic;

namespace ChengYuan.Authorization;

public sealed class PermissionGroupDefinition
{
    private readonly List<PermissionDefinition> _permissions = [];

    internal PermissionGroupDefinition(string name, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public IReadOnlyList<PermissionDefinition> Permissions => _permissions;

    public PermissionDefinition AddPermission(
        string name,
        string? displayName = null,
        MultiTenancySides multiTenancySide = MultiTenancySides.Both,
        bool isEnabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var permission = new PermissionDefinition(name, displayName, multiTenancySide, isEnabled, this);
        _permissions.Add(permission);
        return permission;
    }
}

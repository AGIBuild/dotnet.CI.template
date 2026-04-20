using System;
using System.Collections.Generic;

namespace ChengYuan.Authorization;

public sealed class PermissionDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);
    private readonly List<PermissionDefinition> _children = [];

    internal PermissionDefinition(
        string name,
        string? displayName = null,
        MultiTenancySides multiTenancySide = MultiTenancySides.Both,
        bool isEnabled = true,
        PermissionGroupDefinition? group = null,
        PermissionDefinition? parent = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        DisplayName = displayName;
        MultiTenancySide = multiTenancySide;
        IsEnabled = isEnabled;
        Group = group;
        Parent = parent;
    }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool DefaultGranted { get; set; }

    public bool IsEnabled { get; set; }

    public MultiTenancySides MultiTenancySide { get; set; }

    public PermissionGroupDefinition? Group { get; }

    public PermissionDefinition? Parent { get; }

    public IReadOnlyList<PermissionDefinition> Children => _children;

    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

    public PermissionDefinition AddChild(
        string name,
        string? displayName = null,
        MultiTenancySides multiTenancySide = MultiTenancySides.Both,
        bool isEnabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var child = new PermissionDefinition(name, displayName, multiTenancySide, isEnabled, Group, this);
        _children.Add(child);
        return child;
    }

    public bool TryGetMetadata(string name, out object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _metadata.TryGetValue(name, out value);
    }

    internal void SetMetadata(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _metadata[name] = value;
    }
}

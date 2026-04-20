using System;
using System.Collections.Generic;

namespace ChengYuan.Core.UI.Navigation;

public sealed class MenuItemDefinition
{
    private readonly List<MenuItemDefinition> _children = [];

    public MenuItemDefinition(string name, string displayName, string? url = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Name = name;
        DisplayName = displayName;
        Url = url;
    }

    public string Name { get; }

    public string DisplayName { get; set; }

    public string? Url { get; set; }

    public string? Icon { get; set; }

    public int Order { get; set; } = 1000;

    public string? RequiredPermission { get; set; }

    public IReadOnlyList<MenuItemDefinition> Children => _children;

    public MenuItemDefinition AddChild(MenuItemDefinition child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
        return this;
    }

    public MenuItemDefinition? FindChild(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        foreach (var child in _children)
        {
            if (string.Equals(child.Name, name, StringComparison.Ordinal))
            {
                return child;
            }

            var found = child.FindChild(name);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}

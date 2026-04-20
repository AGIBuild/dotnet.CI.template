using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Core.UI.Navigation;

public sealed class ApplicationMenu
{
    private const string AdministrationMenuName = "Administration";

    private readonly List<MenuItemDefinition> _items = [];

    public ApplicationMenu(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<MenuItemDefinition> Items => _items;

    public ApplicationMenu AddItem(MenuItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
        return this;
    }

    public MenuItemDefinition? FindItem(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        foreach (var item in _items)
        {
            if (string.Equals(item.Name, name, StringComparison.Ordinal))
            {
                return item;
            }

            var found = item.FindChild(name);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    public MenuItemDefinition GetAdministration()
    {
        var administration = _items.FirstOrDefault(
            static item => string.Equals(item.Name, AdministrationMenuName, StringComparison.Ordinal));

        if (administration is null)
        {
            administration = new MenuItemDefinition(AdministrationMenuName, "Administration")
            {
                Icon = "SettingOutlined",
                Order = 900,
            };
            _items.Add(administration);
        }

        return administration;
    }
}

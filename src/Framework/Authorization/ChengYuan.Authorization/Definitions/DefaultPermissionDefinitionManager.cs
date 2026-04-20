using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Authorization;

internal sealed class DefaultPermissionDefinitionManager(
    IEnumerable<IPermissionDefinitionContributor> contributors) : IPermissionDefinitionManager
{
    private readonly object _syncLock = new();
    private readonly Dictionary<string, PermissionGroupDefinition> _groups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PermissionDefinition> _permissions = new(StringComparer.Ordinal);
    private bool _initialized;

    public IReadOnlyList<PermissionGroupDefinition> GetGroups()
    {
        EnsureInitialized();
        return _groups.Values.ToArray();
    }

    public PermissionGroupDefinition? GetGroupOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _groups.GetValueOrDefault(name);
    }

    public PermissionGroupDefinition GetGroup(string name)
    {
        return GetGroupOrNull(name)
            ?? throw new InvalidOperationException($"Permission group '{name}' is not defined.");
    }

    public PermissionDefinition? GetOrNull(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _permissions.GetValueOrDefault(name);
    }

    public PermissionDefinition GetPermission(string name)
    {
        return GetOrNull(name)
            ?? throw new InvalidOperationException($"Permission '{name}' is not defined.");
    }

    public IReadOnlyCollection<PermissionDefinition> GetAll()
    {
        EnsureInitialized();
        return _permissions.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _permissions.ContainsKey(name);
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_syncLock)
        {
            if (_initialized) return;

            var context = new DefinitionContext(this);
            foreach (var contributor in contributors)
            {
                contributor.Define(context);
            }

            // Index all permissions from all groups
            foreach (var group in _groups.Values)
            {
                foreach (var permission in group.Permissions)
                {
                    IndexPermission(permission);
                }
            }

            _initialized = true;
        }
    }

    private void IndexPermission(PermissionDefinition permission)
    {
        _permissions[permission.Name] = permission;
        foreach (var child in permission.Children)
        {
            IndexPermission(child);
        }
    }

    private sealed class DefinitionContext(DefaultPermissionDefinitionManager manager) : IPermissionDefinitionContext
    {
        public PermissionGroupDefinition AddGroup(string name, string? displayName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.ContainsKey(name))
            {
                throw new InvalidOperationException($"Permission group '{name}' is already defined.");
            }

            var group = new PermissionGroupDefinition(name, displayName);
            manager._groups[name] = group;
            return group;
        }

        public PermissionGroupDefinition? GetGroupOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._groups.GetValueOrDefault(name);
        }

        public PermissionGroupDefinition GetGroup(string name)
        {
            return GetGroupOrNull(name)
                ?? throw new InvalidOperationException($"Permission group '{name}' is not defined.");
        }

        public PermissionDefinition? GetPermissionOrNull(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return manager._permissions.GetValueOrDefault(name);
        }

        public void RemoveGroup(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (manager._groups.Remove(name, out var group))
            {
                foreach (var permission in group.Permissions)
                {
                    RemovePermissionRecursive(permission);
                }
            }
        }

        private void RemovePermissionRecursive(PermissionDefinition permission)
        {
            manager._permissions.Remove(permission.Name);
            foreach (var child in permission.Children)
            {
                RemovePermissionRecursive(child);
            }
        }
    }
}

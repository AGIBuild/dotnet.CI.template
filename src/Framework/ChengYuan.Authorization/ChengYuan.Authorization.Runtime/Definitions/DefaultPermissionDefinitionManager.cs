using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Authorization;

internal sealed class DefaultPermissionDefinitionManager : IPermissionDefinitionManager
{
    private readonly Dictionary<string, PermissionDefinition> _definitions = new(StringComparer.Ordinal);

    public PermissionDefinitionBuilder AddOrUpdate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = new PermissionDefinition(name);
        _definitions[name] = definition;
        return new PermissionDefinitionBuilder(definition);
    }

    public PermissionDefinition? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.GetValueOrDefault(name);
    }

    public PermissionDefinition GetDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Find(name)
            ?? throw new InvalidOperationException($"Permission '{name}' is not defined.");
    }

    public IReadOnlyCollection<PermissionDefinition> GetAll()
    {
        return _definitions.Values.ToArray();
    }

    public bool IsDefined(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _definitions.ContainsKey(name);
    }
}

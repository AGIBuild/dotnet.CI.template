using System;
using System.Collections.Generic;

namespace ChengYuan.Authorization;

public sealed class PermissionDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

    public PermissionDefinition(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }

    public bool DefaultGranted { get; internal set; }

    public string? DisplayName { get; internal set; }

    public string? Description { get; internal set; }

    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

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

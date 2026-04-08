using System;

namespace ChengYuan.Authorization;

public sealed class PermissionDefinitionBuilder
{
    public PermissionDefinitionBuilder(PermissionDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public PermissionDefinition Definition { get; }

    public PermissionDefinitionBuilder WithDefaultGrant(bool isGranted)
    {
        Definition.DefaultGranted = isGranted;
        return this;
    }

    public PermissionDefinitionBuilder WithDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Definition.DisplayName = displayName;
        return this;
    }

    public PermissionDefinitionBuilder WithDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Definition.Description = description;
        return this;
    }

    public PermissionDefinitionBuilder WithMetadata(string name, object? value)
    {
        Definition.SetMetadata(name, value);
        return this;
    }
}

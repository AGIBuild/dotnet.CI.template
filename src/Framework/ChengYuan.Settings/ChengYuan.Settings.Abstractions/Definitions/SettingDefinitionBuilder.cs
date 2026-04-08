using System;

namespace ChengYuan.Settings;

public sealed class SettingDefinitionBuilder
{
    public SettingDefinitionBuilder(SettingDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public SettingDefinition Definition { get; }

    public SettingDefinitionBuilder WithDefaultValue(object? value)
    {
        if (!IsCompatibleWithDefinitionType(Definition.ValueType, value))
        {
            throw new ArgumentException(
                $"Default value for setting '{Definition.Name}' must be assignable to '{Definition.ValueType.FullName}'.",
                nameof(value));
        }

        Definition.DefaultValue = value;
        return this;
    }

    public SettingDefinitionBuilder WithDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Definition.DisplayName = displayName;
        return this;
    }

    public SettingDefinitionBuilder WithDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Definition.Description = description;
        return this;
    }

    public SettingDefinitionBuilder WithMetadata(string name, object? value)
    {
        Definition.SetMetadata(name, value);
        return this;
    }

    private static bool IsCompatibleWithDefinitionType(Type valueType, object? value)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        if (value is null)
        {
            return !valueType.IsValueType || Nullable.GetUnderlyingType(valueType) is not null;
        }

        var targetType = Nullable.GetUnderlyingType(valueType) ?? valueType;
        return targetType.IsInstanceOfType(value);
    }
}

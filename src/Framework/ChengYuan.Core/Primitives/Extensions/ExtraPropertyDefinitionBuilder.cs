namespace ChengYuan.Core.Extensions;

public sealed class ExtraPropertyDefinitionBuilder
{
    internal ExtraPropertyDefinitionBuilder(ExtraPropertyDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public ExtraPropertyDefinition Definition { get; }

    public ExtraPropertyDefinitionBuilder WithDefaultValue(object? value)
    {
        if (!ExtraPropertyManager.IsValueCompatible(Definition.PropertyType, value))
        {
            throw new ArgumentException(
                $"Default value for extra property '{Definition.Name}' must be assignable to '{Definition.PropertyType.FullName}'.",
                nameof(value));
        }

        Definition.DefaultValue = value;
        return this;
    }

    public ExtraPropertyDefinitionBuilder WithDisplayName(string displayName)
    {
        Definition.DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName));
        return this;
    }

    public ExtraPropertyDefinitionBuilder Required(bool isRequired = true)
    {
        Definition.IsRequired = isRequired;
        return this;
    }

    public ExtraPropertyDefinitionBuilder WithMetadata(string name, object? value)
    {
        Definition.SetMetadata(name, value);
        return this;
    }
}

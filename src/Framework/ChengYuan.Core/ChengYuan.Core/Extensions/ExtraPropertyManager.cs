using System.Text.Json;

namespace ChengYuan.Core.Extensions;

public sealed class ExtraPropertyManager
{
    private readonly Dictionary<Type, Dictionary<string, ExtraPropertyDefinition>> _definitions = new();

    public ExtraPropertyDefinitionBuilder AddOrUpdate<TSource, TProperty>(string name)
        where TSource : IHasExtraProperties
    {
        return AddOrUpdate(typeof(TSource), name, typeof(TProperty));
    }

    public ExtraPropertyDefinitionBuilder AddOrUpdate(Type ownerType, string name, Type propertyType)
    {
        ArgumentNullException.ThrowIfNull(ownerType);
        ArgumentNullException.ThrowIfNull(propertyType);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));
        var definitions = GetOrCreateDefinitions(ownerType);
        var definition = new ExtraPropertyDefinition(propertyName, propertyType);
        definitions[propertyName] = definition;
        return new ExtraPropertyDefinitionBuilder(definition);
    }

    public ExtraPropertyDefinition? FindProperty<TSource>(string name)
        where TSource : IHasExtraProperties
    {
        return FindProperty(typeof(TSource), name);
    }

    public ExtraPropertyDefinition? FindProperty(Type ownerType, string name)
    {
        ArgumentNullException.ThrowIfNull(ownerType);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));

        foreach (var candidateType in EnumerateCandidateTypes(ownerType))
        {
            if (_definitions.TryGetValue(candidateType, out var definitions)
                && definitions.TryGetValue(propertyName, out var definition))
            {
                return definition;
            }
        }

        return null;
    }

    public IReadOnlyCollection<ExtraPropertyDefinition> GetProperties<TSource>()
        where TSource : IHasExtraProperties
    {
        return GetProperties(typeof(TSource));
    }

    public IReadOnlyCollection<ExtraPropertyDefinition> GetProperties(Type ownerType)
    {
        ArgumentNullException.ThrowIfNull(ownerType);

        var definitions = new Dictionary<string, ExtraPropertyDefinition>(StringComparer.Ordinal);

        foreach (var candidateType in EnumerateCandidateTypes(ownerType))
        {
            if (!_definitions.TryGetValue(candidateType, out var candidateDefinitions))
            {
                continue;
            }

            foreach (var (name, definition) in candidateDefinitions)
            {
                definitions.TryAdd(name, definition);
            }
        }

        return definitions.Values.ToArray();
    }

    internal static bool IsValueCompatible(Type propertyType, object? value)
    {
        ArgumentNullException.ThrowIfNull(propertyType);

        if (value is null)
        {
            return !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null;
        }

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (targetType.IsInstanceOfType(value))
        {
            return true;
        }

        try
        {
            _ = ConvertValue(value, targetType);
            return true;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException or JsonException or ArgumentException)
        {
            return false;
        }
    }

    internal static object? ConvertValue(object value, Type propertyType)
    {
        ArgumentNullException.ThrowIfNull(propertyType);

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize(jsonElement.GetRawText(), propertyType);
        }

        if (targetType == typeof(Guid) && value is string guidText && Guid.TryParse(guidText, out var guidValue))
        {
            return guidValue;
        }

        if (targetType.IsEnum)
        {
            if (value is string enumText && Enum.TryParse(targetType, enumText, ignoreCase: true, out var parsedEnum))
            {
                return parsedEnum;
            }

            var enumUnderlyingValue = System.Convert.ChangeType(value, Enum.GetUnderlyingType(targetType), System.Globalization.CultureInfo.InvariantCulture);
            return Enum.ToObject(targetType, enumUnderlyingValue!);
        }

        return System.Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
    }

    private Dictionary<string, ExtraPropertyDefinition> GetOrCreateDefinitions(Type ownerType)
    {
        if (_definitions.TryGetValue(ownerType, out var definitions))
        {
            return definitions;
        }

        definitions = new Dictionary<string, ExtraPropertyDefinition>(StringComparer.Ordinal);
        _definitions[ownerType] = definitions;
        return definitions;
    }

    private static IEnumerable<Type> EnumerateCandidateTypes(Type ownerType)
    {
        for (var currentType = ownerType; currentType is not null && currentType != typeof(object); currentType = currentType.BaseType)
        {
            yield return currentType;
        }

        foreach (var interfaceType in ownerType.GetInterfaces())
        {
            yield return interfaceType;
        }
    }
}

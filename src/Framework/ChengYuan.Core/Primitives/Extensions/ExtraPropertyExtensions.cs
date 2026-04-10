using System.Globalization;
using System.Text.Json;

namespace ChengYuan.Core.Extensions;

public static class ExtraPropertyExtensions
{
    public static TSource SetProperty<TSource>(this TSource source, string name, object? value)
        where TSource : IHasExtraProperties
    {
        ArgumentNullException.ThrowIfNull(source);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));
        source.ExtraProperties[propertyName] = value;
        return source;
    }

    public static TSource SetProperty<TSource>(this TSource source, string name, object? value, ExtraPropertyManager manager)
        where TSource : IHasExtraProperties
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));
        var definition = manager.FindProperty(source.GetType(), propertyName);

        if (definition is not null && !ExtraPropertyManager.IsValueCompatible(definition.PropertyType, value))
        {
            throw new InvalidOperationException(
                $"Extra property '{propertyName}' expects values assignable to '{definition.PropertyType.FullName}'.");
        }

        source.ExtraProperties[propertyName] = value;
        return source;
    }

    public static TProperty? GetProperty<TProperty>(this IHasExtraProperties source, string name, TProperty? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));

        if (!source.ExtraProperties.TryGetValue(propertyName, out var value) || value is null)
        {
            return defaultValue;
        }

        if (value is TProperty typedValue)
        {
            return typedValue;
        }

        return ConvertValue<TProperty>(value, defaultValue);
    }

    public static TProperty? GetProperty<TProperty>(this IHasExtraProperties source, string name, ExtraPropertyManager manager, TProperty? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));

        if (source.ExtraProperties.TryGetValue(propertyName, out var value) && value is not null)
        {
            return source.GetProperty<TProperty>(propertyName, defaultValue);
        }

        var definition = manager.FindProperty(source.GetType(), propertyName);

        if (definition?.DefaultValue is null)
        {
            return defaultValue;
        }

        return ConvertValue<TProperty>(definition.DefaultValue, defaultValue);
    }

    public static bool HasProperty(this IHasExtraProperties source, string name)
    {
        ArgumentNullException.ThrowIfNull(source);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));
        return source.ExtraProperties.ContainsKey(propertyName);
    }

    public static bool RemoveProperty(this IHasExtraProperties source, string name)
    {
        ArgumentNullException.ThrowIfNull(source);

        var propertyName = Check.NotNullOrWhiteSpace(name, nameof(name));
        return source.ExtraProperties.Remove(propertyName);
    }

    public static TSource ApplyDefaults<TSource>(this TSource source, ExtraPropertyManager manager)
        where TSource : IHasExtraProperties
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        foreach (var definition in manager.GetProperties(source.GetType()))
        {
            if (definition.DefaultValue is null || source.ExtraProperties.ContainsKey(definition.Name))
            {
                continue;
            }

            source.ExtraProperties[definition.Name] = definition.DefaultValue;
        }

        return source;
    }

    public static TSource ValidateProperties<TSource>(this TSource source, ExtraPropertyManager manager)
        where TSource : IHasExtraProperties
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        foreach (var definition in manager.GetProperties(source.GetType()))
        {
            if (!source.ExtraProperties.TryGetValue(definition.Name, out var value) || value is null)
            {
                if (definition.IsRequired)
                {
                    throw new InvalidOperationException($"Extra property '{definition.Name}' is required.");
                }

                continue;
            }

            if (!ExtraPropertyManager.IsValueCompatible(definition.PropertyType, value))
            {
                throw new InvalidOperationException(
                    $"Extra property '{definition.Name}' expects values assignable to '{definition.PropertyType.FullName}'.");
            }
        }

        return source;
    }

    private static TProperty? ConvertValue<TProperty>(object value, TProperty? defaultValue)
    {
        try
        {
            var convertedValue = ExtraPropertyManager.ConvertValue(value, typeof(TProperty));

            if (convertedValue is null)
            {
                return defaultValue;
            }

            if (convertedValue is TProperty typedValue)
            {
                return typedValue;
            }

            var jsonValue = JsonSerializer.Deserialize<TProperty>(JsonSerializer.Serialize(convertedValue));
            return jsonValue is null ? defaultValue : jsonValue;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException or JsonException)
        {
            throw new InvalidOperationException(
                $"Unable to convert extra property value of type '{value.GetType().FullName}' to '{typeof(TProperty).FullName}'.",
                exception);
        }
    }
}

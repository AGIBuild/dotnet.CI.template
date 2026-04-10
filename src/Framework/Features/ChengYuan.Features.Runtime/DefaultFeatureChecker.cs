using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Features;

internal sealed class DefaultFeatureChecker(
    IFeatureDefinitionManager definitionManager,
    IEnumerable<IFeatureValueProvider> valueProviders,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ICurrentCorrelation currentCorrelation) : IFeatureChecker
{
    private readonly IFeatureValueProvider[] _orderedProviders = valueProviders
        .OrderByDescending(provider => provider.Order)
        .ThenBy(provider => provider.Name, StringComparer.Ordinal)
        .ToArray();

    public async ValueTask<T?> GetAsync<T>(string name, CancellationToken cancellationToken = default)
    {
        var definition = definitionManager.GetDefinition(name);
        EnsureRequestedTypeMatchesDefinition<T>(definition);

        var context = new FeatureContext(currentTenant.Id, currentUser.Id, currentCorrelation.CorrelationId);

        foreach (var provider in _orderedProviders)
        {
            var value = await provider.GetOrNullAsync(definition, context, cancellationToken);
            if (value is not null)
            {
                return ConvertValue<T>(definition, value.Value);
            }
        }

        return ConvertValue<T>(definition, definition.DefaultValue);
    }

    public async ValueTask<T> GetOrDefaultAsync<T>(string name, T defaultValue, CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(name, cancellationToken);
        return value is null ? defaultValue : value;
    }

    public async ValueTask<bool> IsEnabledAsync(string name, CancellationToken cancellationToken = default)
    {
        return await GetOrDefaultAsync(name, false, cancellationToken);
    }

    private static void EnsureRequestedTypeMatchesDefinition<T>(FeatureDefinition definition)
    {
        var requestedType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        var definedType = Nullable.GetUnderlyingType(definition.ValueType) ?? definition.ValueType;

        if (requestedType != definedType)
        {
            throw new InvalidOperationException(
                $"Feature '{definition.Name}' is defined as '{definedType.FullName}' and cannot be requested as '{requestedType.FullName}'.");
        }
    }

    private static T? ConvertValue<T>(FeatureDefinition definition, object? value)
    {
        if (value is null)
        {
            return default;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        try
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }

            if (targetType == typeof(Guid) && value is string guidText && Guid.TryParse(guidText, out var guidValue))
            {
                return (T)(object)guidValue;
            }

            if (targetType.IsEnum)
            {
                if (value is string enumText && Enum.TryParse(targetType, enumText, ignoreCase: true, out var parsedEnum))
                {
                    return (T)parsedEnum;
                }

                var enumUnderlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(targetType), CultureInfo.InvariantCulture);
                return (T)Enum.ToObject(targetType, enumUnderlyingValue!);
            }

            return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException or JsonException)
        {
            throw new InvalidOperationException(
                $"Unable to convert feature '{definition.Name}' value of type '{value.GetType().FullName}' to '{targetType.FullName}'.",
                exception);
        }
    }
}

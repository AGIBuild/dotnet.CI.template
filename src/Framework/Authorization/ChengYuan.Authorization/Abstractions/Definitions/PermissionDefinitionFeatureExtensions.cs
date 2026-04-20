using System;
using System.Linq;

namespace ChengYuan.Authorization;

public static class PermissionDefinitionFeatureExtensions
{
    private const string RequiredFeaturesKey = "RequiredFeatures";

    public static PermissionDefinition RequireFeatures(this PermissionDefinition definition, params string[] featureNames)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (featureNames.Length == 0)
        {
            return definition;
        }

        var existing = definition.GetRequiredFeatures();
        var merged = existing.Length == 0 ? featureNames : existing.Union(featureNames, StringComparer.Ordinal).ToArray();

        definition.SetMetadata(RequiredFeaturesKey, merged);
        return definition;
    }

    public static string[] GetRequiredFeatures(this PermissionDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.TryGetMetadata(RequiredFeaturesKey, out var value) && value is string[] features
            ? features
            : [];
    }
}

using System;

namespace ChengYuan.Core.GlobalFeatures;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequiresGlobalFeatureAttribute : Attribute
{
    public RequiresGlobalFeatureAttribute(Type featureType)
    {
        ArgumentNullException.ThrowIfNull(featureType);

        if (!typeof(GlobalFeature).IsAssignableFrom(featureType))
        {
            throw new ArgumentException($"Type '{featureType.FullName}' must be a subclass of GlobalFeature.", nameof(featureType));
        }

        FeatureType = featureType;
    }

    public Type FeatureType { get; }
}

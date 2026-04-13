using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ChengYuan.Core.GlobalFeatures;

public static class GlobalFeatureManager
{
    private static readonly ConcurrentDictionary<Type, GlobalFeature> Features = new();

    public static TFeature GetOrAdd<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFeature>()
        where TFeature : GlobalFeature, new()
    {
        return (TFeature)Features.GetOrAdd(typeof(TFeature), _ => new TFeature());
    }

    public static void Enable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFeature>()
        where TFeature : GlobalFeature, new()
    {
        GetOrAdd<TFeature>().Enable();
    }

    public static void Disable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFeature>()
        where TFeature : GlobalFeature, new()
    {
        GetOrAdd<TFeature>().Disable();
    }

    public static bool IsEnabled<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFeature>()
        where TFeature : GlobalFeature, new()
    {
        return Features.TryGetValue(typeof(TFeature), out var feature) && feature.IsEnabled;
    }

    public static IReadOnlyCollection<GlobalFeature> GetAll()
    {
        return Features.Values.ToArray();
    }
}

using ChengYuan.Core.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public static class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
        where TModule : ModuleBase, new()
    {
        return services.AddModule(typeof(TModule));
    }

    public static IServiceCollection AddModule(this IServiceCollection services, Type rootModuleType)
    {
        ArgumentNullException.ThrowIfNull(rootModuleType);

        if (!typeof(ModuleBase).IsAssignableFrom(rootModuleType))
        {
            throw new InvalidOperationException($"Module type '{rootModuleType.FullName}' must inherit from {nameof(ModuleBase)}.");
        }

        var orderedNodes = ResolveGraph(rootModuleType);
        var orderedDescriptors = BuildDescriptors(orderedNodes, rootModuleType);
        var catalog = new ModuleCatalog(orderedDescriptors);

        services.AddSingleton(catalog);
        services.AddSingleton<IModuleCatalog>(catalog);
        services.AddSingleton<IModuleManager>(serviceProvider => new ModuleManager(catalog, serviceProvider));

        foreach (var descriptor in orderedDescriptors)
        {
            if (descriptor.Instance is IPreConfigureServices preConfigureServices)
            {
                preConfigureServices.PreConfigureServices(services);
            }

            descriptor.Instance.ConfigureServices(services);

            if (descriptor.Instance is IPostConfigureServices postConfigureServices)
            {
                postConfigureServices.PostConfigureServices(services);
            }
        }

        return services;
    }

    private static List<(Type ModuleType, Type[] DependencyTypes)> ResolveGraph(Type rootModuleType)
    {
        var orderedNodes = new List<(Type ModuleType, Type[] DependencyTypes)>();
        var visitingModules = new HashSet<Type>();
        var visitedModules = new HashSet<Type>();

        Visit(rootModuleType, orderedNodes, visitingModules, visitedModules);
        return orderedNodes;
    }

    private static void Visit(
        Type moduleType,
        ICollection<(Type ModuleType, Type[] DependencyTypes)> orderedNodes,
        ISet<Type> visitingModules,
        ISet<Type> visitedModules)
    {
        if (visitedModules.Contains(moduleType))
        {
            return;
        }

        if (!visitingModules.Add(moduleType))
        {
            throw new InvalidOperationException($"Circular module dependency detected around '{moduleType.FullName}'.");
        }

        var dependencyTypes = GetDependencies(moduleType).ToArray();

        foreach (var dependencyType in dependencyTypes)
        {
            Visit(dependencyType, orderedNodes, visitingModules, visitedModules);
        }

        visitingModules.Remove(moduleType);
        visitedModules.Add(moduleType);
        orderedNodes.Add((moduleType, dependencyTypes));
    }

    private static List<ModuleDescriptor> BuildDescriptors(
        List<(Type ModuleType, Type[] DependencyTypes)> orderedNodes,
        Type rootModuleType)
    {
        var descriptorMap = new Dictionary<Type, ModuleDescriptor>();
        var descriptors = new List<ModuleDescriptor>();

        foreach (var (moduleType, dependencyTypes) in orderedNodes)
        {
            var instance = (ModuleBase)Activator.CreateInstance(moduleType)!;
            var dependencies = dependencyTypes
                .Select(type => (IModuleDescriptor)descriptorMap[type])
                .ToArray();

            var descriptor = new ModuleDescriptor(
                moduleType,
                instance,
                dependencies,
                isRoot: moduleType == rootModuleType);

            descriptorMap[moduleType] = descriptor;
            descriptors.Add(descriptor);
        }

        return descriptors;
    }

    private static IEnumerable<Type> GetDependencies(Type moduleType)
    {
        foreach (var attribute in moduleType.GetCustomAttributes(typeof(DependsOnAttribute), inherit: true).Cast<DependsOnAttribute>())
        {
            foreach (var dependencyType in attribute.ModuleTypes)
            {
                if (!typeof(ModuleBase).IsAssignableFrom(dependencyType))
                {
                    throw new InvalidOperationException(
                        $"Module '{moduleType.FullName}' depends on '{dependencyType.FullName}', which is not a valid module type.");
                }

                yield return dependencyType;
            }
        }
    }
}

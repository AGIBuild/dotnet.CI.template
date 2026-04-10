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

        var orderedModules = OrderModules(rootModuleType);
        services.AddSingleton(new ModuleCatalog(orderedModules));

        foreach (var moduleDescriptor in orderedModules)
        {
            var module = (ModuleBase)Activator.CreateInstance(moduleDescriptor.ModuleType)!;

            if (module is IPreConfigureServices preConfigureServices)
            {
                preConfigureServices.PreConfigureServices(services);
            }

            module.ConfigureServices(services);

            if (module is IPostConfigureServices postConfigureServices)
            {
                postConfigureServices.PostConfigureServices(services);
            }
        }

        return services;
    }

    private static List<ModuleDescriptor> OrderModules(Type rootModuleType)
    {
        var orderedModules = new List<ModuleDescriptor>();
        var visitingModules = new HashSet<Type>();
        var visitedModules = new HashSet<Type>();

        Visit(rootModuleType, orderedModules, visitingModules, visitedModules);
        return orderedModules;
    }

    private static void Visit(
        Type moduleType,
        ICollection<ModuleDescriptor> orderedModules,
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
            Visit(dependencyType, orderedModules, visitingModules, visitedModules);
        }

        visitingModules.Remove(moduleType);
        visitedModules.Add(moduleType);
        orderedModules.Add(new ModuleDescriptor(moduleType, dependencyTypes));
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

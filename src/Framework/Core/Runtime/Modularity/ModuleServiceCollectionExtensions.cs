using ChengYuan.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public static class ModuleServiceCollectionExtensions
{
    public static Task<IServiceCollection> AddModuleAsync<TModule>(this IServiceCollection services)
        where TModule : ModuleBase, new()
    {
        return services.AddModuleAsync(typeof(TModule), []);
    }

    public static Task<IServiceCollection> AddModuleAsync(this IServiceCollection services, Type rootModuleType)
    {
        return services.AddModuleAsync(rootModuleType, []);
    }

    public static async Task<IServiceCollection> AddModuleAsync(
        this IServiceCollection services,
        Type rootModuleType,
        IEnumerable<Type> additionalModuleTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(rootModuleType);
        ArgumentNullException.ThrowIfNull(additionalModuleTypes);

        var (catalog, configurationContext) = PrepareModuleGraph(services, rootModuleType, additionalModuleTypes);

        foreach (var descriptor in catalog.ConcreteModules)
        {
            await ExecuteServiceRegistrationStageAsync(descriptor, configurationContext);
        }

        return services;
    }

    public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
        where TModule : ModuleBase, new()
    {
        return services.AddModule(typeof(TModule), []);
    }

    public static IServiceCollection AddModule(this IServiceCollection services, Type rootModuleType)
    {
        return services.AddModule(rootModuleType, []);
    }

    public static IServiceCollection AddModule(
        this IServiceCollection services,
        Type rootModuleType,
        IEnumerable<Type> additionalModuleTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(rootModuleType);
        ArgumentNullException.ThrowIfNull(additionalModuleTypes);

        var (catalog, configurationContext) = PrepareModuleGraph(services, rootModuleType, additionalModuleTypes);

        foreach (var descriptor in catalog.ConcreteModules)
        {
            ExecuteServiceRegistrationStage(descriptor, configurationContext);
        }

        return services;
    }

    private static (ModuleCatalog Catalog, ServiceConfigurationContext Context) PrepareModuleGraph(
        IServiceCollection services,
        Type rootModuleType,
        IEnumerable<Type> additionalModuleTypes)
    {
        if (!typeof(ModuleBase).IsAssignableFrom(rootModuleType))
        {
            throw new InvalidOperationException($"Module type '{rootModuleType.FullName}' must inherit from {nameof(ModuleBase)}.");
        }

        var additionalModuleTypeArray = additionalModuleTypes.ToArray();
        foreach (var moduleType in additionalModuleTypeArray)
        {
            if (!typeof(ModuleBase).IsAssignableFrom(moduleType))
            {
                throw new InvalidOperationException($"Module type '{moduleType.FullName}' must inherit from {nameof(ModuleBase)}.");
            }
        }

        var initLoggerFactory = new DefaultInitLoggerFactory();

        var orderedNodes = ResolveGraph(rootModuleType, additionalModuleTypeArray);
        var orderedDescriptors = BuildDescriptors(orderedNodes, rootModuleType);
        var dependentsMap = BuildDependentsMap(orderedDescriptors);
        var catalog = new ModuleCatalog(orderedDescriptors);

        services.AddSingleton(catalog);
        services.AddSingleton<IModuleCatalog>(catalog);
        services.AddSingleton<IModuleManager>(serviceProvider => new ModuleManager(catalog, serviceProvider));
        services.AddSingleton<IInitLoggerFactory>(initLoggerFactory);

        foreach (var descriptor in orderedDescriptors)
        {
            services.AddSingleton(descriptor.ModuleType, descriptor.Instance);
        }

        foreach (var descriptor in orderedDescriptors)
        {
            ExecuteLoadStage(descriptor, catalog, dependentsMap);
        }

        var configurationContext = new ServiceConfigurationContext(services, initLoggerFactory, ResolveConfiguration(services));

        return (catalog, configurationContext);
    }

    private static IConfiguration? ResolveConfiguration(IServiceCollection services)
    {
        return services
            .LastOrDefault(static d => d.ServiceType == typeof(IConfiguration))
            ?.ImplementationInstance as IConfiguration;
    }

    private static Dictionary<Type, IReadOnlyList<IModuleDescriptor>> BuildDependentsMap(
        IReadOnlyList<ModuleDescriptor> descriptors)
    {
        var dependents = descriptors.ToDictionary(
            static descriptor => descriptor.ModuleType,
            static _ => new List<IModuleDescriptor>());

        foreach (var descriptor in descriptors)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                dependents[dependency.ModuleType].Add(descriptor);
            }
        }

        return dependents.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<IModuleDescriptor>)pair.Value);
    }

    private static void ExecuteLoadStage(
        ModuleDescriptor descriptor,
        ModuleCatalog catalog,
        Dictionary<Type, IReadOnlyList<IModuleDescriptor>> dependentsMap)
    {
        try
        {
            var loadContext = new ModuleLoadContext(
                descriptor,
                catalog,
                dependentsMap[descriptor.ModuleType]);

            descriptor.Instance.OnLoaded(loadContext);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred during load of module '{descriptor.Name}'.",
                ex);
        }
    }

    private static void ExecuteServiceRegistrationStage(ModuleDescriptor descriptor, ServiceConfigurationContext context)
    {
        try
        {
            descriptor.Instance.ServiceConfigurationContext = context;
            descriptor.Instance.PreConfigureServices(context);
            descriptor.Instance.ConfigureServices(context);
            descriptor.Instance.PostConfigureServices(context);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred during service registration of module '{descriptor.Name}'.",
                ex);
        }
        finally
        {
            descriptor.Instance.ServiceConfigurationContext = null!;
        }
    }

    private static async Task ExecuteServiceRegistrationStageAsync(ModuleDescriptor descriptor, ServiceConfigurationContext context)
    {
        try
        {
            descriptor.Instance.ServiceConfigurationContext = context;
            await descriptor.Instance.PreConfigureServicesAsync(context);
            await descriptor.Instance.ConfigureServicesAsync(context);
            await descriptor.Instance.PostConfigureServicesAsync(context);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred during service registration of module '{descriptor.Name}'.",
                ex);
        }
        finally
        {
            descriptor.Instance.ServiceConfigurationContext = null!;
        }
    }

    private static List<(Type ModuleType, Type[] DependencyTypes)> ResolveGraph(
        Type rootModuleType,
        IReadOnlyCollection<Type> additionalModuleTypes)
    {
        var orderedNodes = new List<(Type ModuleType, Type[] DependencyTypes)>();
        var visitingModules = new HashSet<Type>();
        var visitedModules = new HashSet<Type>();

        foreach (var additionalModuleType in additionalModuleTypes)
        {
            if (additionalModuleType == rootModuleType)
            {
                continue;
            }

            Visit(additionalModuleType, orderedNodes, visitingModules, visitedModules);
        }

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

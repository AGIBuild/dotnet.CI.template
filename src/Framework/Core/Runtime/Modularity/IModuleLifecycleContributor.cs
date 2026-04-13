using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public interface IModuleLifecycleContributor
{
    Task InitializeAsync(IModuleInitializationContext context, IModuleDescriptor descriptor, ModuleBase instance);

    Task ShutdownAsync(IModuleShutdownContext context, IModuleDescriptor descriptor, ModuleBase instance);
}

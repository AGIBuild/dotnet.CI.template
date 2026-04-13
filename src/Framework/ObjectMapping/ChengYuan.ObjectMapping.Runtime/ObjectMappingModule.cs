using ChengYuan.Core.Modularity;

namespace ChengYuan.ObjectMapping;

public sealed class ObjectMappingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddObjectMapping();
    }
}

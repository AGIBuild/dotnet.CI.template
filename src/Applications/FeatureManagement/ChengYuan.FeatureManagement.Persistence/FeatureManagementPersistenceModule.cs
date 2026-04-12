using ChengYuan.Core.Modularity;

namespace ChengYuan.FeatureManagement;

[DependsOn(typeof(FeatureManagementModule))]
public sealed class FeatureManagementPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddFeatureManagementPersistence();
    }
}

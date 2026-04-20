using ChengYuan.Core.Modularity;

namespace ChengYuan.FeatureManagement;

[DependsOn(typeof(FeatureManagementPersistenceModule))]
public sealed class FeatureManagementWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddFeatureManagementWeb();
    }
}

using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FeatureManagement;

[DependsOn(typeof(FeatureManagementModule))]
public sealed class FeatureManagementPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddFeatureManagementPersistence();
    }
}

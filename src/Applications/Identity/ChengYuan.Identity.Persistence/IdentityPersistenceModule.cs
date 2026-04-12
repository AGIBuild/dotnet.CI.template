using ChengYuan.Core.Modularity;

namespace ChengYuan.Identity;

[DependsOn(typeof(IdentityModule))]
public sealed class IdentityPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddIdentityPersistence();
    }
}

using ChengYuan.Core.Modularity;

namespace ChengYuan.Identity;

[DependsOn(typeof(IdentityPersistenceModule))]
public sealed class IdentityWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddIdentityWeb();
    }
}

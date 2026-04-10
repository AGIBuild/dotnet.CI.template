using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(IdentityModule))]
internal sealed class IdentityTestModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<InMemoryIdentityRoleRepository>();
        services.AddSingleton<InMemoryIdentityUserRepository>();
        services.AddSingleton<IIdentityRoleRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryIdentityRoleRepository>());
        services.AddSingleton<IIdentityUserRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryIdentityUserRepository>());
        services.AddSingleton<IUnitOfWork, NoopUnitOfWork>();
    }
}

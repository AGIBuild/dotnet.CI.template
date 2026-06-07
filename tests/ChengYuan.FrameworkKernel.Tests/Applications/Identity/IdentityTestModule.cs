using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(IdentityModule))]
internal sealed class IdentityTestModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<InMemoryIdentityRoleRepository>();
        context.Services.AddSingleton<InMemoryIdentityUserRepository>();
        context.Services.AddSingleton<InMemoryIdentityUserTenantMembershipRepository>();
        context.Services.AddSingleton<IIdentityRoleRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryIdentityRoleRepository>());
        context.Services.AddSingleton<IIdentityUserRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryIdentityUserRepository>());
        context.Services.AddSingleton<IIdentityUserTenantMembershipRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryIdentityUserTenantMembershipRepository>());
        context.Services.AddSingleton<IUnitOfWork, NoopUnitOfWork>();
    }
}

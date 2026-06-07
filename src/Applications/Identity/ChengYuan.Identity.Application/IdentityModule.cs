using ChengYuan.Core;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

[DependsOn(typeof(CoreRuntimeModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class IdentityModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddIdentityApplication();
        context.Services.AddSingleton(ResolveAdminSeedOptions(context));
    }

    private static IdentityAdminSeedOptions ResolveAdminSeedOptions(ServiceConfigurationContext context)
    {
        var section = context.Configuration?.GetSection("Identity:Admin");
        var seedEnabled = string.Equals(section?["SeedEnabled"], "true", StringComparison.OrdinalIgnoreCase);
        if (!seedEnabled)
        {
            return IdentityAdminSeedOptions.Disabled;
        }

        var userName = section?["UserName"];
        var email = section?["Email"];
        var password = section?["Password"];
        var roleName = section?["RoleName"];

        if (string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(roleName))
        {
            throw new InvalidOperationException(
                "Identity admin seeding is enabled, but Identity:Admin must define UserName, Email, Password, and RoleName.");
        }

        return new IdentityAdminSeedOptions(
            SeedEnabled: true,
            UserName: userName,
            Email: email,
            Password: password,
            RoleName: roleName);
    }
}

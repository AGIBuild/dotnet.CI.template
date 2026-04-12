using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

[DependsOn(typeof(WebHostFrameworkCompositionModule))]
internal sealed class WebHostHttpCompositionModule : HostModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();

        new HttpTenantResolutionBuilder(context.Services)
            .AddDefaultSources();
    }
}
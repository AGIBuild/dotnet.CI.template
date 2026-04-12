using ChengYuan.Core.Modularity;

namespace ChengYuan.WebHost;

[DependsOn(typeof(WebHostFrameworkCompositionModule))]
[DependsOn(typeof(WebHostApplicationCompositionModule))]
[DependsOn(typeof(WebHostHttpCompositionModule))]
internal sealed class WebHostModule : HostModule
{
}

using ChengYuan.Core.Modularity;

namespace ChengYuan.CliHost;

[DependsOn(typeof(CliHostFrameworkCompositionModule))]
[DependsOn(typeof(CliHostApplicationCompositionModule))]
[DependsOn(typeof(CliHostRuntimeGlueModule))]
internal sealed class CliHostModule : HostModule
{
}
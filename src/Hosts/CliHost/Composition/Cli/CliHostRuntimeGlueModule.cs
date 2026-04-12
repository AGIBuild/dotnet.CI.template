using ChengYuan.Core.Modularity;

namespace ChengYuan.CliHost;

[DependsOn(typeof(CliHostFrameworkCompositionModule))]
internal sealed class CliHostRuntimeGlueModule : HostModule
{
}
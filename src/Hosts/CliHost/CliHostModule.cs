using ChengYuan.Core.Modularity;

namespace ChengYuan.CliHost;

/// <summary>
/// Root module for CLI host composition.
/// </summary>
[DependsOn(typeof(CliHostRuntimeGlueModule))]
internal sealed class CliHostModule : HostModule
{
}

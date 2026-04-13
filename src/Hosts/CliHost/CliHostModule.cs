using ChengYuan.Core.Modularity;

namespace ChengYuan.CliHost;

/// <summary>
/// Minimal root module for CLI host. Does not depend on any framework composition —
/// all capabilities are declared explicitly via <c>AddModule&lt;T&gt;()</c> in Program.cs.
/// </summary>
internal sealed class CliHostModule : HostModule
{
}

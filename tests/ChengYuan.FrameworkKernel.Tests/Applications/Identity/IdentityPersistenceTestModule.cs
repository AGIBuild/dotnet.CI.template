using ChengYuan.Core.Modularity;
using ChengYuan.Identity;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(IdentityPersistenceModule))]
internal sealed class IdentityPersistenceTestModule : ModuleBase
{
}

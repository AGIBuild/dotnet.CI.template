using ChengYuan.Core.Modularity;
using ChengYuan.PermissionManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(PermissionManagementPersistenceModule))]
internal sealed class PermissionManagementPersistenceTestModule : ModuleBase
{
}

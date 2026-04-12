using ChengYuan.Core.Modularity;
using ChengYuan.TenantManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(TenantManagementPersistenceModule))]
internal sealed class TenantManagementPersistenceTestModule : ExtensionModule
{
}

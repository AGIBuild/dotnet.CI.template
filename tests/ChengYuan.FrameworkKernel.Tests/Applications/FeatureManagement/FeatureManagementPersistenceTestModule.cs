using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(FeatureManagementPersistenceModule))]
internal sealed class FeatureManagementPersistenceTestModule : ExtensionModule
{
}

using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(AuditLoggingPersistenceModule))]
internal sealed class AuditLoggingPersistenceTestModule : ExtensionModule
{
}

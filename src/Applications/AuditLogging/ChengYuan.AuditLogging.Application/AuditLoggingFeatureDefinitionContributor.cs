using ChengYuan.Features;

namespace ChengYuan.AuditLogging;

internal sealed class AuditLoggingFeatureDefinitionContributor : IFeatureDefinitionContributor
{
    public void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AuditLoggingFeatures.GroupName, "Audit Logging");

        group.AddFeature<bool>(AuditLoggingFeatures.EnableAuditLogging, true, "Enable audit logging");
    }
}

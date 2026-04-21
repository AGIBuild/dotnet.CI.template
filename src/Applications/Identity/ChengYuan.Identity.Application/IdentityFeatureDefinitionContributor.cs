using ChengYuan.Features;

namespace ChengYuan.Identity;

internal sealed class IdentityFeatureDefinitionContributor : IFeatureDefinitionContributor
{
    public void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(IdentityFeatures.GroupName, "Identity");

        group.AddFeature<bool>(IdentityFeatures.EnableUserRegistration, true, "Enable user registration");
    }
}

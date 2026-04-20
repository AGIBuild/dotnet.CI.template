namespace ChengYuan.Features;

public interface IFeatureDefinitionContext
{
    FeatureGroupDefinition AddGroup(string name, string? displayName = null);

    FeatureGroupDefinition? GetGroupOrNull(string name);

    FeatureGroupDefinition GetGroup(string name);

    FeatureDefinition? GetFeatureOrNull(string name);

    void RemoveGroup(string name);
}

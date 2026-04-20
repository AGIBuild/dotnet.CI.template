using System.Collections.Generic;

namespace ChengYuan.Features;

public interface IFeatureDefinitionManager
{
    IReadOnlyList<FeatureGroupDefinition> GetGroups();

    FeatureGroupDefinition? GetGroupOrNull(string name);

    FeatureGroupDefinition GetGroup(string name);

    FeatureDefinition? GetOrNull(string name);

    FeatureDefinition GetFeature(string name);

    IReadOnlyCollection<FeatureDefinition> GetAll();

    bool IsDefined(string name);
}

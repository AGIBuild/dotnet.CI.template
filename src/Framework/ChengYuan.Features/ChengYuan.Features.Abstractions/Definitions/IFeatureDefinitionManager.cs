using System.Collections.Generic;

namespace ChengYuan.Features;

public interface IFeatureDefinitionManager
{
    FeatureDefinitionBuilder AddOrUpdate<TValue>(string name);

    FeatureDefinition? Find(string name);

    FeatureDefinition GetDefinition(string name);

    IReadOnlyCollection<FeatureDefinition> GetAll();

    bool IsDefined(string name);
}

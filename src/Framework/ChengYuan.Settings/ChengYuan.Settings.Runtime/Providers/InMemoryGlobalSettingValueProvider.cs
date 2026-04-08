using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class InMemoryGlobalSettingValueProvider(IReadOnlyDictionary<string, object?> values) : ISettingValueProvider
{
    public string Name => "InMemoryGlobal";

    public int Order => 150;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return ValueTask.FromResult(
            values.TryGetValue(definition.Name, out var value)
                ? new SettingValue(value, Name)
                : null);
    }
}

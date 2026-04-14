using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class InMemoryUserSettingValueProvider(IReadOnlyDictionary<(string UserId, string Name), object?> values) : ISettingValueProvider
{
    public string Name => "InMemoryUser";

    public int Order => 350;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            return ValueTask.FromResult<SettingValue?>(null);
        }

        return ValueTask.FromResult(
            values.TryGetValue((context.UserId, definition.Name), out var value)
                ? new SettingValue(value, Name)
                : null);
    }
}

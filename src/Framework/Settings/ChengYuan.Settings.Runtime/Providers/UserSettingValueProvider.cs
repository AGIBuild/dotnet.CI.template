using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class UserSettingValueProvider : ISettingValueProvider
{
    public string Name => "User";

    public int Order => 300;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<SettingValue?>(null);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class TenantSettingValueProvider : ISettingValueProvider
{
    public string Name => "Tenant";

    public int Order => 200;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<SettingValue?>(null);
    }
}

using System.Globalization;
using ChengYuan.Core.Results;

namespace ChengYuan.Core.Localization;

public sealed class ErrorLocalizer(IEnumerable<ILocalizationResource> resources) : IErrorLocalizer
{
    private readonly IReadOnlyList<ILocalizationResource> _resources = resources.ToArray();

    public string Localize(ResultError resultError, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(resultError);

        var effectiveCulture = culture ?? CultureInfo.CurrentUICulture;

        foreach (var resource in _resources)
        {
            if (resource.TryGetString(resultError.Code, effectiveCulture, out var localizedValue))
            {
                return localizedValue!;
            }
        }

        return resultError.Message;
    }
}

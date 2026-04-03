using System.Globalization;

namespace ChengYuan.Core.Localization;

public interface ILocalizationResource
{
    string Name { get; }

    bool TryGetString(string key, CultureInfo culture, out string? value);
}

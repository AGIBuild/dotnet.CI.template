using System.Globalization;
using ChengYuan.Core.Results;

namespace ChengYuan.Core.Localization;

public interface IErrorLocalizer
{
    string Localize(ResultError resultError, CultureInfo? culture = null);
}

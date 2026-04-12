using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public interface IHasLogLevel
{
    LogLevel LogLevel { get; set; }
}

using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Logging;

public interface IExceptionWithSelfLogging
{
    void Log(ILogger logger);
}

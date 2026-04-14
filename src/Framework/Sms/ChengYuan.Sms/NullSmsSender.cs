using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Sms;

internal sealed partial class NullSmsSender(ILogger<NullSmsSender> logger) : ISmsSender
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        LogSmsSkipped(logger, message.PhoneNumber, message.Text);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SMS to '{PhoneNumber}' with text '{Text}' was not sent (NullSmsSender is active).")]
    private static partial void LogSmsSkipped(ILogger logger, string phoneNumber, string text);
}

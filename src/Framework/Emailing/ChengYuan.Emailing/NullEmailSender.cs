using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Emailing;

internal sealed partial class NullEmailSender(ILogger<NullEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        LogEmailSkipped(logger, message.To, message.Subject);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Email to '{To}' with subject '{Subject}' was not sent (NullEmailSender is active).")]
    private static partial void LogEmailSkipped(ILogger logger, string to, string subject);
}

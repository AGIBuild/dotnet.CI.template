using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Emailing;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

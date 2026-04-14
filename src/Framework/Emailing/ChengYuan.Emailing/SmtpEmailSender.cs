using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ChengYuan.Emailing;

internal sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using var mailMessage = BuildMailMessage(message);
        using var client = CreateSmtpClient();

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    private MailMessage BuildMailMessage(EmailMessage message)
    {
        var from = message.From ?? _options.DefaultFromAddress;
        var mailMessage = new MailMessage
        {
            From = new MailAddress(from, _options.DefaultFromDisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        mailMessage.To.Add(message.To);

        foreach (var cc in message.Cc)
        {
            mailMessage.CC.Add(cc);
        }

        foreach (var bcc in message.Bcc)
        {
            mailMessage.Bcc.Add(bcc);
        }

        return mailMessage;
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrEmpty(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        return client;
    }
}

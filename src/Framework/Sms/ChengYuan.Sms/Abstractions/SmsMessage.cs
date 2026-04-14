using System;

namespace ChengYuan.Sms;

public sealed class SmsMessage
{
    public SmsMessage(string phoneNumber, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        PhoneNumber = phoneNumber;
        Text = text;
    }

    public string PhoneNumber { get; }

    public string Text { get; }

    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

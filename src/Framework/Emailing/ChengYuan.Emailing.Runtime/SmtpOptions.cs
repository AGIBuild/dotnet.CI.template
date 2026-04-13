namespace ChengYuan.Emailing;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 25;

    public bool UseSsl { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string DefaultFromAddress { get; set; } = "noreply@chengyuan.dev";

    public string? DefaultFromDisplayName { get; set; }
}

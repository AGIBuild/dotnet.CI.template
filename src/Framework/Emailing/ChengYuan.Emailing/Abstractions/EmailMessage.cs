using System;
using System.Collections.Generic;

namespace ChengYuan.Emailing;

public sealed class EmailMessage
{
    public required string To { get; init; }

    public IReadOnlyList<string> Cc { get; init; } = [];

    public IReadOnlyList<string> Bcc { get; init; } = [];

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public bool IsHtml { get; init; } = true;

    public string? From { get; init; }
}

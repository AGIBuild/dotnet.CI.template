using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;

namespace ChengYuan.Auditing;

public sealed class AuditingOptions
{
    public bool IsEnabled { get; set; } = true;

    public bool IsEnabledForAnonymousUsers { get; set; } = true;

    public bool IsEnabledForGetRequests { get; set; }

    public bool AlwaysLogOnException { get; set; } = true;

    public List<Type> IgnoredTypes { get; } =
    [
        typeof(Stream),
        typeof(Expression),
        typeof(CancellationToken),
        typeof(byte[]),
    ];

    public List<Func<Type, bool>> EntityHistorySelectors { get; } = [];
}

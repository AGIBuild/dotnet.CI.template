using System;

namespace ChengYuan.Auditing;

public sealed class AuditMiddlewareOptions
{
    public Func<Microsoft.AspNetCore.Http.HttpContext, bool>? RequestFilter { get; set; }
}

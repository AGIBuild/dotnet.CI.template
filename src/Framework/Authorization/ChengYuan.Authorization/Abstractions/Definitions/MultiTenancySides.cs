using System;

namespace ChengYuan.Authorization;

[Flags]
public enum MultiTenancySides
{
    Host = 1,
    Tenant = 2,
    Both = Host | Tenant
}

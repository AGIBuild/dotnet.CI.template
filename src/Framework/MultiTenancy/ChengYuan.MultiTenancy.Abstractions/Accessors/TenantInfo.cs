using System;

namespace ChengYuan.MultiTenancy;

public sealed record TenantInfo(Guid? Id, string? Name);

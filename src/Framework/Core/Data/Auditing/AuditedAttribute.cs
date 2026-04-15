using System;

namespace ChengYuan.Core.Data.Auditing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditedAttribute : Attribute;

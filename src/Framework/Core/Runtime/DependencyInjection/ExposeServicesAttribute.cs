namespace ChengYuan.Core.DependencyInjection;

/// <summary>
/// Explicitly declares the service types that a class should be registered as
/// during conventional service registration. When present, the naming convention
/// is bypassed and only the listed types are used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ExposeServicesAttribute(params Type[] serviceTypes) : Attribute
{
    public Type[] ServiceTypes { get; } = serviceTypes;
}

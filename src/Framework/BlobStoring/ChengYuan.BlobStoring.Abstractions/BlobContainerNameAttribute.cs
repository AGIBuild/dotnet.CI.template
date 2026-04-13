using System;

namespace ChengYuan.BlobStoring;

[AttributeUsage(AttributeTargets.Class)]
public sealed class BlobContainerNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string GetContainerName<T>()
    {
        return GetContainerName(typeof(T));
    }

    public static string GetContainerName(Type type)
    {
        var attribute = (BlobContainerNameAttribute?)Attribute.GetCustomAttribute(type, typeof(BlobContainerNameAttribute));
        return attribute?.Name ?? type.FullName ?? type.Name;
    }
}

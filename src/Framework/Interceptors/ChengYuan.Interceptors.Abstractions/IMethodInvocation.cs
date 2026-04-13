using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ChengYuan.Interceptors;

public interface IMethodInvocation
{
    MethodInfo Method { get; }

    object TargetObject { get; }

    object?[] Arguments { get; }

    Type[] GenericArguments { get; }

    object? ReturnValue { get; set; }

    Task ProceedAsync();
}

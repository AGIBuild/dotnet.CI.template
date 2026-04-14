using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ChengYuan.Interceptors;

internal sealed class MethodInvocation : IMethodInvocation
{
    private readonly Func<Task> _implementation;
    private readonly IReadOnlyList<IChengYuanInterceptor> _interceptors;
    private int _currentIndex = -1;

    public MethodInvocation(
        MethodInfo method,
        object targetObject,
        object?[] arguments,
        Type[] genericArguments,
        IReadOnlyList<IChengYuanInterceptor> interceptors,
        Func<Task> implementation)
    {
        Method = method;
        TargetObject = targetObject;
        Arguments = arguments;
        GenericArguments = genericArguments;
        _interceptors = interceptors;
        _implementation = implementation;
    }

    public MethodInfo Method { get; }

    public object TargetObject { get; }

    public object?[] Arguments { get; }

    public Type[] GenericArguments { get; }

    public object? ReturnValue { get; set; }

    public async Task ProceedAsync()
    {
        _currentIndex++;

        if (_currentIndex < _interceptors.Count)
        {
            await _interceptors[_currentIndex].InterceptAsync(this);
        }
        else
        {
            await _implementation();
        }
    }
}

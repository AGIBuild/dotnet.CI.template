using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ChengYuan.Interceptors;

public sealed class InterceptorPipeline
{
    private readonly IReadOnlyList<IChengYuanInterceptor> _interceptors;

    public InterceptorPipeline(IEnumerable<IChengYuanInterceptor> interceptors)
    {
        ArgumentNullException.ThrowIfNull(interceptors);
        _interceptors = [.. interceptors];
    }

    public async Task<object?> ExecuteAsync(
        MethodInfo method,
        object targetObject,
        object?[] arguments,
        Func<Task> implementation)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(implementation);

        if (_interceptors.Count == 0)
        {
            await implementation();
            return null;
        }

        var invocation = new MethodInvocation(
            method,
            targetObject,
            arguments,
            method.IsGenericMethod ? method.GetGenericArguments() : Type.EmptyTypes,
            _interceptors,
            implementation);

        await invocation.ProceedAsync();
        return invocation.ReturnValue;
    }
}

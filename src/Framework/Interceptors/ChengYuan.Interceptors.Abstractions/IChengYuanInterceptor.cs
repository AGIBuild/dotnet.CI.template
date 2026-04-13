using System.Threading.Tasks;

namespace ChengYuan.Interceptors;

public interface IChengYuanInterceptor
{
    Task InterceptAsync(IMethodInvocation invocation);
}

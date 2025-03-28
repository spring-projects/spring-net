using AopAlliance.Intercept;

namespace Spring.Aop.Framework;

public class UnsupportedInterceptor : IMethodInterceptor
{
    public object Invoke(IMethodInvocation invocation)
    {
        throw new NotImplementedException(invocation.Method.Name);
    }
}

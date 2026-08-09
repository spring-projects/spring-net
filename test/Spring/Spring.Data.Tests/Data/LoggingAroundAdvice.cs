using AopAlliance.Intercept;
using Microsoft.Extensions.Logging;

namespace Spring.Data;

public class LoggingAroundAdvice : IMethodInterceptor
{
    public int numInvoked;
    private static readonly ILogger<LoggingAroundAdvice> LOG = LogManager.GetLogger<LoggingAroundAdvice>();

    public object Invoke(IMethodInvocation invocation)
    {
        try
        {
            LOG.LogDebug("Advice executing; calling the advised method [{MethodName}]", invocation.Method.Name);
            object returnValue = invocation.Proceed();
            LOG.LogDebug("Advice executed; advised method [{MethodName}] returned {ReturnValue}", invocation.Method.Name, returnValue);
            return returnValue;
        }
        finally
        {
            numInvoked++;
        }
    }
}

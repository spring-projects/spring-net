using AopAlliance.Intercept;
using Microsoft.Extensions.Logging;

namespace Spring.Data
{
    public class ConsoleLoggingAroundAdvice : IMethodInterceptor
    {
        private static readonly ILog LOG = LogManager.GetLogger<ConsoleLoggingAroundAdvice>();
        public object Invoke(IMethodInvocation invocation)
        {
            LOG.LogDebug("Advice executing; calling the advised method [" + invocation.Method.Name + "]");
            object returnValue = invocation.Proceed();
            LOG.LogDebug("Advice executed; advised method [" + invocation.Method.Name + "] returned " + returnValue);
            return returnValue;
        }
    }
}

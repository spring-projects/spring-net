
using AopAlliance.Intercept;
using Common.Logging;

namespace Spring.Data
{
    public class LoggingAroundAdvice : IMethodInterceptor
    {
        public int numInvoked;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LoggingAroundAdvice));
        public object Invoke(IMethodInvocation invocation)
        {
            try
            {
                LOG.Debug("Advice executing; calling the advised method [" + invocation.Method.Name + "]");
                object returnValue = invocation.Proceed();
                LOG.Debug("Advice executed; advised method [" + invocation.Method.Name + "] returned " + returnValue);
                return returnValue;
            } finally
            {
                numInvoked++;
            }
        }
    }
}

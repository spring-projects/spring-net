using System;
using System.Reflection;
using AopAlliance.Intercept;
using Common.Logging;

namespace Spring.WcfQuickStart
{
    public class SimplePerformanceInterceptor : IMethodInterceptor
    {
        #region Logging Definition

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        private string prefix = "Invocation took";


        public string Prefix
        {
            get { return prefix; }
            set { prefix = value; }
        }

        #region IMethodInterceptor Members

        public object Invoke(IMethodInvocation invocation)
        {
            DateTime start = DateTime.Now;
            try
            {
                return invocation.Proceed();
            }
            finally
            {
                DateTime stop = DateTime.Now;

                TimeSpan time = stop - start;
                log.Info(Prefix + " " + time);
            }
        }

        #endregion
    }
}

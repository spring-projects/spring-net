using System;
using System.Diagnostics;
using System.Reflection;

using AopAlliance.Intercept;

namespace Spring.WcfQuickStart
{
    public class SimplePerformanceInterceptor : IMethodInterceptor
    {
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
                Trace.WriteLine(Prefix + " " + time);
            }
        }

        #endregion
    }
}

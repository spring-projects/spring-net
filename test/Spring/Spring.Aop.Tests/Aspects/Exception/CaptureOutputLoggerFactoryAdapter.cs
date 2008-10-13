

using System;
using System.Collections.Specialized;
using Common.Logging;

namespace Spring.Aspects.Exceptions
{
    public class CaptureOutputLoggerFactoryAdapter : ILoggerFactoryAdapter
    {
        private CaptureOutputLogger adviceLogger;

        public CaptureOutputLoggerFactoryAdapter()
        {
        }

        public CaptureOutputLoggerFactoryAdapter(NameValueCollection properties)
        {
        }


        public CaptureOutputLogger AdviceLogger
        {
            get { return adviceLogger; }
        }

        #region ILoggerFactoryAdapter Members

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            CaptureOutputLogger logger = new CaptureOutputLogger();
            if (name.Equals("adviceHandler") || name.IndexOf("LogExceptionHandler") >= 0)
            {
                adviceLogger = logger;
            }
            
            return logger;
        }

        #endregion
    }
}
using System;
using Spring.Messaging.Nms.Core;
using Common.Logging;

namespace Spring.Messaging.Nms.Integration
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggingExceptionHandler : IExceptionListener
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (LoggingExceptionHandler));

        #endregion


        #region IExceptionListener Members

        public void OnException(Exception e)
        {
            LOG.Error("Exception processing message", e);            
        }

        #endregion
    }
}


using System;
using Common.Logging;
using Spring.Messaging.Ems.Core;
using TIBCO.EMS;

namespace Spring.EmsQuickStart.Server.Handlers
{
    public class LoggingExceptionListener : IExceptionListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(LoggingExceptionListener));

        #endregion

        public void OnException(EMSException exception)
        {
            logger.Info("********* Caught exception *************", exception);
        }
    }
}
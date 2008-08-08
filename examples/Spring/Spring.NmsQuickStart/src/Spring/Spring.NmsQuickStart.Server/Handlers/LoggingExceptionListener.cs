

using System;
using Common.Logging;
using Spring.Messaging.Nms.Core;

namespace Spring.NmsQuickStart.Server.Handlers
{
    public class LoggingExceptionListener : IExceptionListener
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(LoggingExceptionListener));

        #endregion

        /// <summary>
        /// Called when there is an exception in message processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void OnException(Exception exception)
        {
            logger.Info("********* Caught exception *************", exception);
        }
    }
}
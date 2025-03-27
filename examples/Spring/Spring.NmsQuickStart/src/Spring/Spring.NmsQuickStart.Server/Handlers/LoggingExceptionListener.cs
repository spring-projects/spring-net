

using System;
using Microsoft.Extensions.Logging;
using Spring.Messaging.Nms.Core;

namespace Spring.NmsQuickStart.Server.Handlers
{
    public class LoggingExceptionListener : IExceptionListener
    {
        #region Logging

        private readonly ILogger logger = LogManager.GetLogger(typeof(LoggingExceptionListener));

        #endregion

        /// <summary>
        /// Called when there is an exception in message processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void OnException(Exception exception)
        {
            logger.LogInformation(exception, "********* Caught exception *************");
        }
    }
}



using System;
using System.Messaging;
using Common.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleExceptionHandler : IExceptionHandler
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleExceptionHandler));

        #endregion

        private int messageCount;


        public int MessageCount
        {
            get { return messageCount; }
            set { messageCount = value; }
        }

        #region IExceptionHandler Members

        public void OnException(Exception exception, Message message)
        {
            LOG.Error("Exception Handler processing message id = [" + message.Id + "]");
            LOG.Error("Exception = ", exception);
            messageCount++;
        }

        #endregion
    }
}
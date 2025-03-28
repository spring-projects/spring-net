

using System.Messaging;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleMessageListener : IMessageListener
    {
        #region Logging Definition

        private static readonly ILogger<SimpleMessageListener> LOG = LogManager.GetLogger<SimpleMessageListener>();
        #endregion

        private Message lastReceivedMessage;
        private int messageCount;

        public Message LastReceivedMessage
        {
            get { return lastReceivedMessage; }
        }


        public int MessageCount
        {
            get { return messageCount; }
        }

        #region IMessageListener Members

        public void OnMessage(Message message)
        {
            lastReceivedMessage = message;
            messageCount++;
            LOG.LogDebug("Message listener count = " + messageCount);
        }

        #endregion
    }
}

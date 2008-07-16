

using System.Messaging;
using Common.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleMessageListener : IMessageListener
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleMessageListener));
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
            LOG.Debug("Message listener count = " + messageCount);
        }

        #endregion
    }
}
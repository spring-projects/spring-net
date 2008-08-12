using Common.Logging;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Integration
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
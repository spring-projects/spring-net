

using Spring.Messaging.Nms;
using Apache.NMS;
using Common.Logging;

namespace Spring.Messaging.Nms.Integration
{
    public class SimpleMessageListener : IMessageListener
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleMessageListener));
        #endregion

        private IMessage lastReceivedMessage;
        private int messageCount;

        public IMessage LastReceivedMessage
        {
            get { return lastReceivedMessage; }
        }


        public int MessageCount
        {
            get { return messageCount; }
        }

        #region IMessageListener Members

        public void OnMessage(IMessage message)
        {
            lastReceivedMessage = message;
            messageCount++;
            LOG.Debug("Message listener count = " + messageCount);
        }

        #endregion
    }
}
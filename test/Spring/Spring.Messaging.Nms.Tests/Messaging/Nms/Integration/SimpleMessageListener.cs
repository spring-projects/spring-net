

using Spring.Messaging.Nms.Core;
using Apache.NMS;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Nms.Integration
{
    public class SimpleMessageListener : IMessageListener
    {
        #region Logging Definition

        private static readonly ILogger<SimpleMessageListener> LOG = LogManager.GetLogger<SimpleMessageListener>();
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
            LOG.LogDebug("Message listener count = " + messageCount);
        }

        #endregion
    }
}

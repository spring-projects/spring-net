using Common.Logging;

namespace Spring.Messaging.Ems.Core
{
    public class SimpleMessageListener : IMessageListener
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleMessageListener));

        private int messageCount;

        public int MessageCount
        {
            get { return messageCount; }
        }

        public void OnMessage(Message message)
        {           
            messageCount++;
            LOG.Debug("Message listener count = " + messageCount);
            TextMessage textMessage = message as TextMessage;
            if (textMessage != null)
            {
                LOG.Info("Message Text = " + textMessage.Text);
            } else
            {
                LOG.Warn("Can not process message of type " + message.GetType());
            }
        }
    }
}
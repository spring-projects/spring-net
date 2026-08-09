using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Ems.Core;

public class SimpleMessageListener : IMessageListener
{
    private static readonly ILogger<SimpleMessageListener> LOG = LogManager.GetLogger<SimpleMessageListener>();

    private int messageCount;

    public int MessageCount
    {
        get { return messageCount; }
    }

    public void OnMessage(Message message)
    {
        messageCount++;
        LOG.LogDebug("Message listener count = {MessageCount}", messageCount);
        TextMessage textMessage = message as TextMessage;
        if (textMessage != null)
        {
            LOG.LogInformation("Message Text = {Text}", textMessage.Text);
        }
        else
        {
            LOG.LogWarning("Can not process message of type {MessageType}", message.GetType());
        }
    }
}

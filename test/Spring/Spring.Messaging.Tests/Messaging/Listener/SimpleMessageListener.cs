using System.Messaging;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener;

public class SimpleMessageListener : IMessageListener
{
    private static readonly ILogger<SimpleMessageListener> LOG = LogManager.GetLogger<SimpleMessageListener>();

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

    public void OnMessage(Message message)
    {
        lastReceivedMessage = message;
        messageCount++;
        LOG.LogDebug("Message listener count = " + messageCount);
    }
}
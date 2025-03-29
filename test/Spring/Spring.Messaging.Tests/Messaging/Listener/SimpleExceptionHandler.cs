using System.Messaging;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener;

public class SimpleExceptionHandler : IExceptionHandler
{
    private static readonly ILogger<SimpleExceptionHandler> LOG = LogManager.GetLogger<SimpleExceptionHandler>();

    private int messageCount;

    public int MessageCount
    {
        get { return messageCount; }
        set { messageCount = value; }
    }

    public void OnException(Exception exception, Message message)
    {
        LOG.LogError("Exception Handler processing message id = [" + message.Id + "]");
        LOG.LogError(exception, "Exception = ");
        messageCount++;
    }
}

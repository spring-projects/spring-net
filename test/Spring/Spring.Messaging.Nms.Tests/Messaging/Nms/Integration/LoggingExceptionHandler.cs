using Microsoft.Extensions.Logging;
using Spring.Messaging.Nms.Core;

namespace Spring.Messaging.Nms.Integration;

/// <summary>
///
/// </summary>
public class LoggingExceptionHandler : IExceptionListener
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(LoggingExceptionHandler));

    public void OnException(Exception e)
    {
        LOG.LogError(e, "Exception processing message");
    }
}

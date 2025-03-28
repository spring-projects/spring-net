using Microsoft.Extensions.Logging;
using Spring.Messaging.Nms.Core;

namespace Spring.Messaging.Nms.Integration
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggingExceptionHandler : IExceptionListener
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (LoggingExceptionHandler));

        #endregion


        #region IExceptionListener Members

        public void OnException(Exception e)
        {
            LOG.LogError(e, "Exception processing message");            
        }

        #endregion
    }
}

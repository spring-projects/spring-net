using Common.Logging;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Integration
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

        public void OnException(EMSException e)
        {
            LOG.Error("Exception processing message", e);            
        }

        #endregion

    }
}
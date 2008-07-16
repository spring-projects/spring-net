using System;
using System.Messaging;
using System.Threading;
using Common.Logging;

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggingExceptionHandler : IExceptionHandler
    {
        private TimeSpan recoveryTimeSpan;

        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (LoggingExceptionHandler));

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExceptionHandler"/> class with
        /// a default recovery time span of 5 seconds.
        /// </summary>
        public LoggingExceptionHandler()
        {
            recoveryTimeSpan = new TimeSpan(0, 0, 0, 5);
        }

        public LoggingExceptionHandler(TimeSpan recoveryTimeSpan)
        {
            this.recoveryTimeSpan = recoveryTimeSpan;
        }

        public TimeSpan RecoveryTimeSpan
        {
            set { recoveryTimeSpan = value; }
        }

        #region IExceptionListener Members

        public void OnException(Exception exception, Message message)
        {
            //TODO other exception handling
            MessageQueueException e = exception as MessageQueueException;
            if (e != null)
            {
                switch ((int) e.MessageQueueErrorCode)
                {
                    case (int) MessageQueueErrorCode.IOTimeout:
                    case -1073741536:
                        Console.WriteLine("Msmq Error -1073741536 or IOTimeout : Sleeping, and then ReListening");
                        Thread.Sleep(recoveryTimeSpan);
                        break;
                    default:
                        LOG.Error("Exception Receiving Message", e);
                        break;
                }
            }
            else
            {
                LOG.Error("got exception", exception);
            }
        }

        #endregion
    }
}


using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;

namespace Spring.Aspects.Exceptions
{
    public class CaptureOutputLogger : TraceLogger
    {
        public static readonly string NAME = "capturingLogger";
#if NET_1_0 || NET_1_1
        public CaptureOutputLogger() 
            : base(NAME, LogLevel.All, false, false, null)
        {}
#else
		public CaptureOutputLogger()
			: base(false, NAME, LogLevel.All, true, false, false, null)
		{ }
#endif

//        private LogLevel _currentLogLevel = LogLevel.All;
//
//        private IList logMessages = new ArrayList();
//
//        public IList LogMessages
//        {
//            get { return logMessages; }
//            set { logMessages = value; }
//        }
//
//        /// <summary>
//        /// Do the actual logging by constructing the log message using a <see cref="StringBuilder" /> then
//        /// sending the output to <see cref="Console.Out" />.
//        /// </summary>
//        /// <param name="level">The <see cref="LogLevel" /> of the message.</param>
//        /// <param name="message">The log message.</param>
//        /// <param name="e">An optional <see cref="Exception" /> associated with the message.</param>
//        private void Write(LogLevel level, object message, Exception e)
//        {
//            // Use a StringBuilder for better performance
//            StringBuilder sb = new StringBuilder();
//            // Append date-time if so configured
//            // Append a readable representation of the log level
//            sb.Append(("[" + level.ToString().ToUpper() + "]").PadRight(8));
//
//            // Append the message
//            sb.Append(message);
//
//            // Append stack trace if not null
//            if (e != null)
//            {
//                sb.Append(Environment.NewLine).Append(e.ToString());
//            }
//
//            // Print to the appropriate destination
//            logMessages.Add(sb.ToString());
//        }
//
//        /// <summary>
//        /// Determines if the given log level is currently enabled.
//        /// </summary>
//        /// <param name="level"></param>
//        /// <returns></returns>
//        private bool IsLevelEnabled(LogLevel level)
//        {
//            int iLevel = (int)level;
//            int iCurrentLogLevel = (int)_currentLogLevel;
//
//            // return iLevel.CompareTo(iCurrentLogLevel); better ???
//            return (iLevel >= iCurrentLogLevel);
//        }
//
//        #region ILog Members
//
//        public void Trace(object message)
//        {
//            Trace(message, null);
//        }
//
//        public void Trace(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Trace))
//            {
//                Write(LogLevel.Trace, message, e);
//            }
//        }
//
//        public void Debug(object message)
//        {
//            Debug(message, null);
//        }
//
//        public void Debug(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Debug))
//            {
//                Write(LogLevel.Debug, message, e);
//            }
//        }
//
//        public void Error(object message)
//        {
//            Error(message, null);
//        }
//
//        public void Error(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Error))
//            {
//                Write(LogLevel.Error, message, e);
//            }
//        }
//
//        public void Fatal(object message)
//        {
//            Fatal(message, null);
//        }
//
//        public void Fatal(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Fatal))
//            {
//                Write(LogLevel.Fatal, message, e);
//            }
//        }
//
//        public void Info(object message)
//        {
//            Info(message, null);
//        }
//
//        public void Info(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Info))
//            {
//                Write(LogLevel.Info, message, e);
//            }
//        }
//
//        public void Warn(object message)
//        {
//            Warn(message, null);
//        }
//
//        public void Warn(object message, Exception e)
//        {
//            if (IsLevelEnabled(LogLevel.Warn))
//            {
//                Write(LogLevel.Warn, message, e);
//            }
//        }
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Trace" />. If it is, all messages will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsTraceEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Trace); }
//        }
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Debug" />. If it is, all messages will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsDebugEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Debug); }
//        }
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Error" />. If it is, only messages with a <see cref="LogLevel" /> of
//        /// <see cref="LogLevel.Error" /> and <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsErrorEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Error); }
//        }
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Fatal" />. If it is, only messages with a <see cref="LogLevel" /> of
//        /// <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsFatalEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Fatal); }
//        }
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Info" />. If it is, only messages with a <see cref="LogLevel" /> of
//        /// <see cref="LogLevel.Info" />, <see cref="LogLevel.Warn" />, <see cref="LogLevel.Error" />, and 
//        /// <see cref="LogLevel.Fatal" /> will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsInfoEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Info); }
//        }
//
//
//        /// <summary>
//        /// Returns <see langword="true" /> if the current <see cref="LogLevel" /> is greater than or
//        /// equal to <see cref="LogLevel.Warn" />. If it is, only messages with a <see cref="LogLevel" /> of
//        /// <see cref="LogLevel.Warn" />, <see cref="LogLevel.Error" />, and <see cref="LogLevel.Fatal" /> 
//        /// will be sent to <see cref="Console.Out" />.
//        /// </summary>
//        public bool IsWarnEnabled
//        {
//            get { return IsLevelEnabled(LogLevel.Warn); }
//        }
//
//        #endregion
    }
}
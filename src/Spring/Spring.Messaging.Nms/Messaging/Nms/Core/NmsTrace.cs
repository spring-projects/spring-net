#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using Apache.NMS;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// Implemention of NMS ITrace interface that will log NMS messages to Common.Logging.
    /// </summary>
    /// <remarks>Registering of this class is done by default in NmsTemplate and SimpleMessageListenerContainer if the value
    /// of Apache.NMS.Tracer.Trace is null, indicating it was not set.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class NmsTrace : ITrace
    {
        #region Logging Definition

        private readonly ILog log;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsTrace"/> class. The log name used is typeof(NmsTrace).
        /// </summary>
        public NmsTrace()
        {
            log = LogManager.GetLogger(typeof(NmsTrace));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="NmsTrace"/> class.
        /// </summary>
        /// <param name="log">The log instance to use for logging.</param>
        public NmsTrace(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Logs message at Debug Level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            log.LogDebug(message);
        }

        /// <summary>
        /// Logs message at Info Level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message)
        {
            log.LogInformation(message);
        }

        /// <summary>
        /// Logs message at Warn Level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn(string message)
        {
            log.LogWarning(message);
        }

        /// <summary>
        /// Logs message at Error Level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message)
        {
            log.LogError(message);
        }

        /// <summary>
        /// Logs message at Fatal Level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Fatal(string message)
        {
            log.LogCritical(message);
        }

        /// <summary>
        /// Gets a value indicating whether the debug log level is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugEnabled
        {
            get { return log.IsEnabled(LogLevel.Debug); }
        }

        /// <summary>
        /// Gets a value indicating whether the info log level is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is info enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsInfoEnabled
        {
            get { return log.IsEnabled(LogLevel.Information); }
        }

        /// <summary>
        /// Gets a value indicating whether the warn log level is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is warn enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsWarnEnabled
        {
            get { return log.IsEnabled(LogLevel.Warning); }
        }

        /// <summary>
        /// Gets a value indicating whether the error log level is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is error enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsErrorEnabled
        {
            get { return log.IsEnabled(LogLevel.Error); }
        }

        /// <summary>
        /// Gets a value indicating whether the fatal log level is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is fatal enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsFatalEnabled
        {
            get { return log.IsEnabled(LogLevel.Critical); }
        }
    }

}

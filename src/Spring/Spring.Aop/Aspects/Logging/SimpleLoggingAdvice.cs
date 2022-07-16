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

using System.Reflection;
using System.Text;
using AopAlliance.Intercept;
using Common.Logging;

namespace Spring.Aspects.Logging
{
    /// <summary>
    /// Configurable advice for logging.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    [Serializable]
    public class SimpleLoggingAdvice : AbstractLoggingAdvice
    {
        #region Fields

        /// <summary>
        /// Flag to indicate if unique identifier should be in the log message.
        /// </summary>
        private bool logUniqueIdentifier;

        /// <summary>
        /// Flag to indicate if the execution time should be in the log message.
        /// </summary>
        private bool logExecutionTime;

        /// <summary>
        /// Flag to indicate if the method arguments should be in the log message.
        /// </summary>
        private bool logMethodArguments;

        /// <summary>
        /// Flag to indicate if the return value should be in the log message.
        /// </summary>
        private bool logReturnValue;

        /// <summary>
        /// The separator string to use for delmiting log message fields.
        /// </summary>
        private string separator = ", ";

        /// <summary>
        /// The log level to use for logging the entry, exit, exception messages.
        /// </summary>
        private LogLevel logLevel = LogLevel.Trace;


        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLoggingAdvice"/> class.
        /// </summary>
        public SimpleLoggingAdvice()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLoggingAdvice"/> class.
        /// </summary>
        /// <param name="useDynamicLogger">if set to <c>true</c> to use dynamic logger, if
        /// <c>false</c> use static logger.</param>
        public SimpleLoggingAdvice(bool useDynamicLogger)
        {
            UseDynamicLogger = useDynamicLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLoggingAdvice"/> class.
        /// </summary>
        /// <param name="defaultLogger">the default logger to use</param>
        public SimpleLoggingAdvice(ILog defaultLogger)
            : base(defaultLogger)
        {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to log a unique identifier with the log message.
        /// </summary>
        /// <value><c>true</c> if [log unique identifier]; otherwise, <c>false</c>.</value>
        public bool LogUniqueIdentifier
        {
            get { return logUniqueIdentifier; }
            set { logUniqueIdentifier = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log execution time.
        /// </summary>
        /// <value><c>true</c> if log execution time; otherwise, <c>false</c>.</value>
        public bool LogExecutionTime
        {
            get { return logExecutionTime; }
            set { logExecutionTime = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether log method arguments.
        /// </summary>
        /// <value><c>true</c> if log method arguments]; otherwise, <c>false</c>.</value>
        public bool LogMethodArguments
        {
            get { return logMethodArguments; }
            set { logMethodArguments = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether log return value.
        /// </summary>
        /// <value><c>true</c> if log return value; otherwise, <c>false</c>.</value>
        public bool LogReturnValue
        {
            get { return logReturnValue; }
            set { logReturnValue = value; }
        }

        /// <summary>
        /// Gets or sets the seperator string to use for delmiting log message fields.
        /// </summary>
        /// <value>The seperator.</value>
        public string Separator
        {
            get { return separator; }
            set { separator = value; }
        }

        /// <summary>
        /// Gets or sets the entry log level.
        /// </summary>
        /// <value>The entry log level.</value>
        public LogLevel LogLevel
        {
            get { return logLevel; }
            set { logLevel = value; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Subclasses must override this method to perform any tracing around the supplied
        /// IMethodInvocation.
        /// </summary>
        /// <param name="invocation">The method invocation to log</param>
        /// <param name="log">The log to write messages to</param>
        /// <returns>
        /// The result of the call to IMethodInvocation.Proceed()
        /// </returns>
        /// <remarks>
        /// Subclasses are resonsible for ensuring that the IMethodInvocation actually executes
        /// by calling IMethodInvocation.Proceed().
        /// <para>
        /// By default, the passed-in ILog instance will have log level
        /// "trace" enabled. Subclasses do not have to check for this again, unless
        /// they overwrite the IsInterceptorEnabled method to modify
        /// the default behavior.
        /// </para>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        protected override object InvokeUnderLog(IMethodInvocation invocation, ILog log)
        {
            object returnValue = null;
            bool exitThroughException = false;

            DateTime startTime = DateTime.Now;
            string uniqueIdentifier = null;

            if (LogUniqueIdentifier)
            {
                uniqueIdentifier = CreateUniqueIdentifier();
            }
            try
            {
                WriteToLog(LogLevel, log, GetEntryMessage(invocation, uniqueIdentifier), null);
                returnValue = invocation.Proceed();
                return returnValue;
            } catch (Exception e)
            {
                TimeSpan executionTimeSpan = DateTime.Now - startTime;
                WriteToLog(LogLevel, log, GetExceptionMessage(invocation, e, executionTimeSpan, uniqueIdentifier), e);
                exitThroughException = true;
                throw;
            }
            finally
            {
                if (!exitThroughException)
                {
                    TimeSpan executionTimeSpan = DateTime.Now - startTime;
                    WriteToLog(LogLevel, log, GetExitMessage(invocation, returnValue, executionTimeSpan, uniqueIdentifier), null);
                }
            }
        }

        /// <summary>
        /// Determines whether the given log is enabled.
        /// </summary>
        /// <param name="log">The log instance to check.</param>
        /// <returns>
        /// 	<c>true</c> if log is for a given log level; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Default is true when the trace level is enabled.  Subclasses may override this
        /// to change the level at which logging occurs, or return true to ignore level
        /// checks.</remarks>
        protected override bool IsLogEnabled(ILog log)
        {
            switch (LogLevel)
            {
                case LogLevel.All:
                case LogLevel.Trace:
                    if (log.IsTraceEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Debug:
                    if (log.IsDebugEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Error:
                    if (log.IsErrorEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Fatal:
                    if (log.IsFatalEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Info:
                    if (log.IsInfoEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Warn:
                    if (log.IsWarnEnabled)
                    {
                        return true;
                    }
                    break;
                case LogLevel.Off:
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// Creates a unique identifier.
        /// </summary>
        /// <remarks>
        /// Default implementation uses Guid.NewGuid().  Subclasses may override to provide an alternative
        /// ID generation implementation.
        /// </remarks>
        /// <returns>A unique identifier</returns>
        protected virtual string CreateUniqueIdentifier()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the entry message to log
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="idString">The id string.</param>
        /// <returns>The entry log message</returns>
        protected virtual string GetEntryMessage(IMethodInvocation invocation, string idString)
        {
            StringBuilder sb = new StringBuilder(128);
            sb.Append("Entering ");
            AppendCommonInformation(sb, invocation, idString);
            if (logMethodArguments)
            {
                sb.Append(GetMethodArgumentAsString(invocation));
            }

            return RemoveLastSeparator(sb.ToString(), Separator);
        }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="invocation">The method invocation.</param>
        /// <param name="e">The thown exception.</param>
        /// <param name="executionTimeSpan">The execution time span.</param>
        /// <param name="idString">The id string.</param>
        /// <returns>The exception log message.</returns>
        protected virtual string GetExceptionMessage(IMethodInvocation invocation, Exception e, TimeSpan executionTimeSpan, string idString)
        {
            StringBuilder sb = new StringBuilder(128);
            sb.Append("Exception thrown in ");
            sb.Append(invocation.Method.Name).Append(Separator);
            AppendCommonInformation(sb, invocation, idString);
            if (LogExecutionTime)
            {
                sb.Append(executionTimeSpan.TotalMilliseconds).Append(" ms");
            }

            return RemoveLastSeparator(sb.ToString(), Separator);
        }


        /// <summary>
        /// Gets the exit log message.
        /// </summary>
        /// <param name="invocation">The method invocation.</param>
        /// <param name="returnValue">The return value.</param>
        /// <param name="executionTimeSpan">The execution time span.</param>
        /// <param name="idString">The id string.</param>
        /// <returns>the exit log message</returns>
        protected virtual string GetExitMessage(IMethodInvocation invocation, object returnValue, TimeSpan executionTimeSpan, string idString)
        {
            StringBuilder sb = new StringBuilder(128);
            sb.Append("Exiting ");
            AppendCommonInformation(sb, invocation, idString);
            if (LogReturnValue && invocation.Method.ReturnType != typeof(void))
            {
                sb.Append("return=").Append(returnValue).Append(Separator);
            }
            if (LogExecutionTime)
            {
                sb.Append(executionTimeSpan.TotalMilliseconds).Append(" ms");
            }
            return RemoveLastSeparator(sb.ToString(), Separator);
        }


        /// <summary>
        /// Appends common information across entry,exit, exception logging
        /// </summary>
        /// <remarks>Add method name and unique identifier if required.</remarks>
        /// <param name="sb">The string buffer building logging message.</param>
        /// <param name="invocation">The method invocation.</param>
        /// <param name="idString">The unique identifier string.</param>
        protected virtual void AppendCommonInformation(StringBuilder sb, IMethodInvocation invocation, string idString)
        {
            sb.Append(invocation.Method.Name);
            if (LogUniqueIdentifier)
            {
                sb.Append(Separator).Append(idString);
            }
            sb.Append(Separator);
        }

        /// <summary>
        /// Gets the method argument as argumen name/value pairs.
        /// </summary>
        /// <param name="invocation">The method invocation.</param>
        /// <returns>string for logging method argument name and values.</returns>
        protected virtual string GetMethodArgumentAsString(IMethodInvocation invocation)
        {
            StringBuilder sb = new StringBuilder(128);
            ParameterInfo[] parameterInfos = invocation.Method.GetParameters();
            object[] argValues = invocation.Arguments;
            for (int i=0; i< parameterInfos.Length; i++)
            {
                sb.Append(parameterInfos[i].Name).Append("=").Append(argValues[i]);
                if (i != parameterInfos.Length) sb.Append("; ");
            }

            return RemoveLastSeparator(sb.ToString(), "; ");
        }

        #endregion

        #region Private Methods

        private string RemoveLastSeparator(string str, string separator)
        {
            if (str.EndsWith(separator))
            {
                return str.Substring(0, str.Length - separator.Length);
            }
            else
            {
                return str;
            }
        }

        private void WriteToLog(LogLevel logLevel, ILog log, string text, Exception e)
        {
            switch (logLevel)
            {
                case LogLevel.All:
                case LogLevel.Trace:
                    if (log.IsTraceEnabled)
                    {
                        if (e == null) log.Trace(text); else log.Trace(text, e);
                    }
                    break;
                case LogLevel.Debug:
                    if (log.IsDebugEnabled)
                    {
                        if (e == null) log.Debug(text); else log.Debug(text, e);
                    }
                    break;
                case LogLevel.Error:
                    if (log.IsErrorEnabled)
                    {
                        if (e == null) log.Error(text); else log.Error(text, e);
                    }
                    break;
                case LogLevel.Fatal:
                    if (log.IsFatalEnabled)
                    {
                        if (e == null) log.Fatal(text); else log.Fatal(text, e);
                    }
                    break;
                case LogLevel.Info:
                    if (log.IsInfoEnabled)
                    {
                        if (e == null) log.Info(text); else log.Info(text, e);
                    }
                    break;
                case LogLevel.Warn:
                    if (log.IsWarnEnabled)
                    {
                        if (e == null) log.Warn(text); else log.Warn(text, e);
                    }
                    break;
                case LogLevel.Off:
                default:
                    break;
            }
        }

        #endregion
    }
}

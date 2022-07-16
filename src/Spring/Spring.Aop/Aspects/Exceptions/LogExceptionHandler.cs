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

using Common.Logging;

using Spring.Expressions;

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// Log the exceptions.  Default log nameis "LogExceptionHandler" and log level is Debug
    /// </summary>
    /// <author>Mark Pollack</author>
    public class LogExceptionHandler : AbstractExceptionHandler
    {
        #region Fields
        private string logName = "LogExceptionHandler";

        private LogLevel logLevel = LogLevel.Trace;

        private bool logMessageOnly = false;

        private bool first = true;

        private string actionExpressionText;

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionHandler"/> class.
        /// </summary>
        public LogExceptionHandler()
        {
            ContinueProcessing = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public LogExceptionHandler(string[] exceptionNames) : base(exceptionNames)
        {
            ContinueProcessing = true;
        }



        /// <summary>
        /// Gets or sets the name of the log.
        /// </summary>
        /// <value>The name of the log.</value>
        public string LogName
        {
            get { return logName; }
            set { logName = value; }
        }


        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public LogLevel LogLevel
        {
            get { return logLevel; }
            set { logLevel = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log message only, and not pass in the
        /// exception to the logging API
        /// </summary>
        /// <value><c>true</c> if log message only; otherwise, <c>false</c>.</value>
        public bool LogMessageOnly
        {
            get { return logMessageOnly; }
            set { logMessageOnly = value; }
        }


        /// <summary>
        /// Gets the action translation expression text.  Overridden to add approprate settings to
        /// the SpEL expression that does the logging so that it depends on the values of LogLevel and
        /// LogMessageOnly.  Those properties must be set to the desired values before calling this method.
        ///
        /// </summary>
        /// <value>The action translation expression.</value>
        public override string ActionExpressionText
        {
            get { return actionExpressionText; }
            set
            {
                if (first)
                {
                    first = false;
                    string textPart1 = "#log." + LogLevel.ToString() + "(" + value;
                    if (logMessageOnly)
                    {
                        actionExpressionText = textPart1 + ")";
                    }
                    else
                    {
                        actionExpressionText = textPart1 + ", #e)";
                    }
                }
            }
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="callContextDictionary">the calling context dictionary</param>
        /// <returns>
        /// The return value from handling the exception, if not rethrown or a new exception is thrown.
        /// </returns>
        public override object HandleException(IDictionary<string, object> callContextDictionary)
        {
            //TODO log name is targettype.
            ILog adviceLogger = LogManager.GetLogger(logName);
            callContextDictionary.Add("log", adviceLogger);
            try
            {
                IExpression expression = Expression.Parse(ActionExpressionText);
                expression.GetValue(null, callContextDictionary);
            }
            catch (Exception e)
            {
                log.Warn("Was not able to evaluate action expression [" + ActionExpressionText + "]", e);
            }
            return "logged";
        }

    }
}

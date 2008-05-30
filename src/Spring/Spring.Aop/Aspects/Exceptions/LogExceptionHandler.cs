#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System;
using System.Collections;
using Common.Logging;
using Spring.Expressions;

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// Log the exceptions.  Default log nameis "LogExceptionHandler" and log level is Debug 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: LogExceptionHandler.cs,v 1.6 2008/02/26 00:03:24 markpollack Exp $</version>
    public class LogExceptionHandler : AbstractExceptionHandler
    {
        private string logName = "LogExceptionHandler";


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
        /// Handles the exception.
        /// </summary>
        /// <param name="callContextDictionary">the calling context dictionary</param>
        /// <returns>
        /// The return value from handling the exception, if not rethrown or a new exception is thrown.
        /// </returns>
        public override object HandleException(IDictionary callContextDictionary)
        {
            ILog log = LogManager.GetLogger(logName);
            callContextDictionary.Add("log", log);
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
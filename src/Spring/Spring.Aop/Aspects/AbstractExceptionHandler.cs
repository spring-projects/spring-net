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

using System.Collections;
using Common.Logging;
using Spring.Expressions;

namespace Spring.Aspects
{
    /// <summary>
    /// An abstract base class providing all necessary functionality for typical IExceptionHandler implementations.
    /// </summary>
    /// <author>Mark Pollack</author>
    public abstract class AbstractExceptionHandler  : IExceptionHandler
    {
        #region Fields

        /// <summary>
        /// The logging instance
        /// </summary>
        protected readonly ILog log;

        private IList sourceExceptionNames = new ArrayList();
        private IList sourceExceptionTypes = new ArrayList();
        private string actionExpressionText;
        private bool continueProcessing = false;
        private string constraintExpressionText;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractExceptionHandler"/> class.
        /// </summary>
        public AbstractExceptionHandler()
        {
            log = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public AbstractExceptionHandler(string[] exceptionNames)
        {
            log = LogManager.GetLogger(GetType());
            foreach (string exceptionName in exceptionNames)
            {
                SourceExceptionNames.Add(exceptionName);
            }
        }

        #endregion

        #region Implementation of IExceptionHandler

        #region Properties

        /// <summary>
        /// Gets the source exception names.
        /// </summary>
        /// <value>The source exception names.</value>
        public virtual IList SourceExceptionNames
        {
            get { return sourceExceptionNames; }
            set { sourceExceptionNames = value; }
        }

        /// <summary>
        /// Gets the source exception types.
        /// </summary>
        /// <value>The source exception types.</value>
        public virtual IList SourceExceptionTypes
        {
            get { return sourceExceptionTypes; }
            set { sourceExceptionTypes = value; }
        }

        /// <summary>
        /// Gets the action translation expression text
        /// </summary>
        /// <value>The action translation expression.</value>
        public virtual string ActionExpressionText
        {
            get { return actionExpressionText; }
            set { actionExpressionText = value; }
        }


        /// <summary>
        /// Gets or sets the constraint expression text.
        /// </summary>
        /// <value>The constraint expression text.</value>
        public string ConstraintExpressionText
        {
            get { return constraintExpressionText; }
            set { constraintExpressionText = value; }
        }

        /// <summary>
        /// Gets a value indicating whether to continue processing.
        /// </summary>
        /// <value><c>true</c> if continue processing; otherwise, <c>false</c>.</value>
        public bool ContinueProcessing
        {
            get { return continueProcessing; }
            set { continueProcessing = value; }
        }

        #endregion

        /// <summary>
        /// Determines whether this instance can handle the exception the specified exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="callContextDictionary">The call context dictionary.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified exception; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandleException(Exception ex, IDictionary<string, object> callContextDictionary)
        {
            if (SourceExceptionNames != null)
            {
                foreach (string exceptionName in SourceExceptionNames)
                {
                    if (ex.GetType().Name.IndexOf(exceptionName) >= 0)
                    {
                        return true;
                    }
                }
            }
            if (ConstraintExpressionText != null)
            {
                bool canProcess;
                try
                {
                    IExpression expression = Expression.Parse(ConstraintExpressionText);
                    canProcess = (bool) expression.GetValue(null, callContextDictionary);
                } catch (InvalidCastException e)
                {
                    log.Warn("Was not able to unbox constraint expression to boolean [" + ConstraintExpressionText + "]", e);
                    return false;
                } catch (Exception e)
                {
                    log.Warn("Was not able to evaluate constraint expression [" + ConstraintExpressionText + "]",e);
                    return false;
                }
                return canProcess;
            }

            return false;
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <returns>The return value from handling the exception, if not rethrown or a new exception is thrown.</returns>
        public abstract object HandleException(IDictionary<string, object> callContextDictionary);

        #endregion
    }
}

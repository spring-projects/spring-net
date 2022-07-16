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

namespace Spring.Aspects
{
    /// <summary>
    /// Sleeps for the appropriate amount of time for an exception.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class RetryExceptionHandler : AbstractExceptionHandler
    {
        #region Fields

        private int maximumRetryCount;

        private bool isDelayBased;

        private TimeSpan delayTimeSpan;
        private string delayRateExpression;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryExceptionHandler"/> class.
        /// </summary>
        public RetryExceptionHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public RetryExceptionHandler(string[] exceptionNames) : base(exceptionNames)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the maximum retry count.
        /// </summary>
        /// <value>The maximum retry count.</value>
        public int MaximumRetryCount
        {
            get { return maximumRetryCount; }
            set { maximumRetryCount = value; }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is delay based.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is delay based; otherwise, <c>false</c>.
        /// </value>
        public bool IsDelayBased
        {
            get { return isDelayBased; }
            set { isDelayBased = value; }
        }

        /// <summary>
        /// Gets or sets the delay time span to sleep after an exception is thrown and a rety is
        /// attempted.
        /// </summary>
        /// <value>The delay time span.</value>
        public TimeSpan DelayTimeSpan
        {
            get { return delayTimeSpan; }
            set { delayTimeSpan = value; }
        }

        /// <summary>
        /// Gets or sets the delay rate expression.
        /// </summary>
        /// <value>The delay rate expression.</value>
        public string DelayRateExpression
        {
            get { return delayRateExpression; }
            set { delayRateExpression = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="callContextDictionary"></param>
        /// <returns>
        /// The return value from handling the exception, if not rethrown or a new exception is thrown.
        /// </returns>
        public override object HandleException(IDictionary<string, object> callContextDictionary)
        {
            return null;
        }

        #endregion
    }
}

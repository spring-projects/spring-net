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

namespace Spring.Aspects
{
    /// <summary>
    /// Handles a thrown exception providing calling context.
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Determines whether this instance can handle the exception the specified exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="callContextDictionary">The call context dictionary.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified exception; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandleException(Exception ex, IDictionary<string, object> callContextDictionary);

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="callContextDictionary">The call context dictionary.</param>
        /// <returns>
        /// The return value from handling the exception, if not rethrown or a new exception is thrown.
        /// </returns>
        object HandleException(IDictionary<string, object> callContextDictionary);

        /// <summary>
        /// Gets the source exception names.
        /// </summary>
        /// <value>The source exception names.</value>
        IList SourceExceptionNames
        {
            get; set;
        }

        /// <summary>
        /// Gets the source exception types.
        /// </summary>
        /// <value>The source exception types.</value>
        IList SourceExceptionTypes
        {
            get; set;
        }

        /// <summary>
        /// Gets the translation expression text
        /// </summary>
        /// <value>The translation expression text</value>
        string ActionExpressionText
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the constraint expression text.
        /// </summary>
        /// <value>The constraint expression text.</value>
        string ConstraintExpressionText
        {
            get; set;
        }

        /// <summary>
        /// Gets a value indicating whether to continue processing.
        /// </summary>
        /// <value><c>true</c> if continue processing; otherwise, <c>false</c>.</value>
        bool ContinueProcessing
        {
            get; set;
        }
    }
}

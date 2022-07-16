#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#region Imports

using System.Runtime.Serialization;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// Exception thrown when an attempt is made to use an unsupported
    /// <see cref="Spring.Aop.IAdvisor"/> or <see cref="AopAlliance.Aop.IAdvice"/>
    /// type.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public class UnknownAdviceTypeException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.UnknownAdviceTypeException"/> class.
        /// </summary>
        public UnknownAdviceTypeException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.UnknownAdviceTypeException"/> class.
        /// </summary>
        /// <param name="advice">The advice that caused the exception.</param>
        public UnknownAdviceTypeException(object advice)
            : base("No adapter for IAdvice of type ["
                   + (advice != null ? advice.GetType().FullName : "null") + "].")
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.UnknownAdviceTypeException"/> class with
        /// the specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public UnknownAdviceTypeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.UnknownAdviceTypeException"/> class with
        /// the specified message and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public UnknownAdviceTypeException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <inheritdoc />
        protected UnknownAdviceTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

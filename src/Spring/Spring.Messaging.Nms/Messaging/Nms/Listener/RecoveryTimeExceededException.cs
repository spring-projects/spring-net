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

#region

using System.Runtime.Serialization;
using Apache.NMS;

#endregion

namespace Spring.Messaging.Nms.Listener
{
    /// <summary>
    /// Exception thrown when the maximum connection recovery time has been exceeded.
    /// </summary>
    /// <author>Mark Pollack</author>
    [Serializable]
    public class RecoveryTimeExceededException : NMSException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryTimeExceededException"/> class.
        /// </summary>
        public RecoveryTimeExceededException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryTimeExceededException"/> class, with the specified message
        /// </summary>
        /// <param name="message">The message.</param>
        public RecoveryTimeExceededException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryTimeExceededException"/> class, with the specified message
        /// and root cause exception
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RecoveryTimeExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryTimeExceededException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected RecoveryTimeExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

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

using System.Runtime.Serialization;

namespace Spring.Pool
{
    /// <summary>
    /// Base class for all pooling exceptions.
    /// </summary>
    /// <author>Federico Spinazzi</author>
    [Serializable]
    public class PoolException : Exception
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Pool.PoolException"/> class.
        /// </summary>
        public PoolException ()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Pool.PoolException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public PoolException (string message) : base (message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Pool.PoolException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="innerException">
        /// The root exception that is being wrapped.
        /// </param>
        public PoolException (string message, Exception innerException) : base (message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Pool.PoolException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected PoolException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }
    }
}

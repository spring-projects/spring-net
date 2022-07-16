#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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
using Spring.Util;

namespace Spring.Messaging.Ems.Connections
{
    /// <summary>
    /// Implementation of Spring IExceptionListener interface that supports
    /// chaining allowing the addition of multiple ExceptionListener instances in order.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ChainedExceptionListener : IExceptionListener
    {
        private ArrayList listeners = new ArrayList(2);

        /// <summary>
        /// Adds the exception listener to the chain
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void AddListener(IExceptionListener listener)
        {
            AssertUtils.ArgumentNotNull(listener, "listener", "ExceptionListener must not be null");
            listeners.Add(listener);
        }

        /// <summary>
        /// Called when an exception occurs in message processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void OnException(EMSException exception)
        {
            foreach (IExceptionListener listener in listeners)
            {
                listener.OnException(exception);
            }
        }

        /// <summary>
        /// Gets the exception listeners as an array.
        /// </summary>
        /// <value>The exception listeners.</value>
        public IExceptionListener[] Listeners
        {
            get
            {
                return (IExceptionListener[]) listeners.ToArray(typeof (IExceptionListener));
            }
        }
    }
}

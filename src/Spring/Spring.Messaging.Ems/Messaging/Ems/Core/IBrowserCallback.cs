

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

using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Core
{
    /// <summary>
    /// Callback for browsing the messages in an EMS queue.
    /// </summary>
    /// <remarks>
    /// To be used with EmsTemplate's callback methods that take a IBrowserCallback argument
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IBrowserCallback
    {

        /// <summary>
        /// Perform operations on the given Session and QueueBrowser
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="browser">The browser.</param>
        /// <returns>The object from working with the Session and QueueBrowser, may be null</returns>
        /// <exception cref="EMSException">If there is any problem when accessing EMS API</exception>
        object DoInEms(ISession session, QueueBrowser browser);
    }
}

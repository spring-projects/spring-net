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

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif


namespace Spring.Messaging.Listener
{
    /// <summary>
    /// The callback invoked when a message is received.
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface IMessageListener
    {
        /// <summary>
        /// Called when message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        void OnMessage(Message message);
    }
}
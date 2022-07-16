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

namespace Spring.Messaging.Support.Converters
{
    /// <summary>
    /// An interface specifying the contract to convert to and from <see cref="Message"/> objects.
    /// </summary>
    public interface IMessageConverter : ICloneable
    {
        /// <summary>
        /// Convert the given object to a Message.
        /// </summary>
        /// <param name="obj">The object to send.</param>
        /// <returns>Message to send</returns>
        Message ToMessage(object obj);

        /// <summary>
        /// Convert the given message to a object.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the object</returns>
        object FromMessage(Message message);
    }
}

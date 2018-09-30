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
    /// An <see cref="IMessageConverter"/> implementation that delegates to an instance of
    /// <see cref="ActiveXMessageFormatter"/> to convert messages.  
    /// </summary>
    /// <author>Mark Pollack</author>
    public class ActiveXMessageConverter : IMessageConverter
    {
        private readonly ActiveXMessageFormatter messageFormatter;


        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveXMessageConverter"/> class.
        /// </summary>
        public ActiveXMessageConverter()
        {
            messageFormatter = new ActiveXMessageFormatter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveXMessageConverter"/> class.
        /// </summary>
        /// <param name="messageFormatter">The message formatter.</param>
        public ActiveXMessageConverter(ActiveXMessageFormatter messageFormatter)
        {
            this.messageFormatter = messageFormatter;
        }

        #region IMessageConverter Members

        /// <summary>
        /// Convert the given object to a Message.
        /// </summary>
        /// <param name="obj">The object to send.</param>
        /// <returns>Message to send</returns>
        public Message ToMessage(object obj)
        {
            Message m = new Message();
            m.Body = obj;
            m.Formatter = messageFormatter;
            return m;
        }

        /// <summary>
        /// Convert the given message to a object.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the object</returns>
        public object FromMessage(Message message)
        {
            message.Formatter = messageFormatter;
            return message.Body;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            ActiveXMessageConverter mc = new ActiveXMessageConverter(messageFormatter.Clone() as ActiveXMessageFormatter);
            return mc;
        }

        #endregion
    }
}
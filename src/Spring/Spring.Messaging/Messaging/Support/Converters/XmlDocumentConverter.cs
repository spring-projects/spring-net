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

using System.Xml;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Support.Converters
{
    /// <summary>
    /// Converts an <see cref="XmlDocument"/> to a Message and vice-versa by using the message's 
    /// body stream. 
    /// </summary>
    /// <author>Mark Pollack</author>
    public class XmlDocumentConverter : IMessageConverter
    {
        #region IMessageConverter Members

        /// <summary>
        /// Convert the given object to a Message.
        /// </summary>
        /// <param name="obj">The object to send.</param>
        /// <returns>Message to send</returns>
        public Message ToMessage(object obj)
        {
            XmlDocument doc = obj as XmlDocument;
            if (doc != null)
            {
                Message m = new Message();
                doc.Save(m.BodyStream);
                return m;
            }
            else
            {
                throw new MessagingException("Expected object to be of type System.Xml.XmlDocument");
            }
        }

        /// <summary>
        /// Convert the given message to a object.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the object</returns>
        public object FromMessage(Message message)
        {
            XmlDocument doc = new XmlDocument(); 
            doc.Load(message.BodyStream);
            return doc;
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
            return new XmlDocumentConverter();
        }

        #endregion
    }
}
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

using System.Collections;

using Apache.NMS;

namespace Spring.Messaging.Nms.Support.Converter
{
    /// <summary> A simple message converter that can handle ITextMessages, IBytesMessages,
    /// IMapMessages, and IObjectMessages. Used as default by NmsTemplate, for
    /// <code>ConvertAndSend</code> and <code>ReceiveAndConvert</code> operations.
    ///
    /// <p>Converts a String to a NMS ITextMessage, a byte array to a NMS IBytesMessage,
    /// a Map to a NMS IMapMessage, and a Serializable object to a NMS IObjectMessage
    /// (or vice versa).</p>
    ///
    ///
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class SimpleMessageConverter : IMessageConverter
    {
        /// <summary> Convert a .NET object to a NMS Message using the supplied session
        /// to create the message object.
        /// </summary>
        /// <param name="objectToConvert">the object to convert
        /// </param>
        /// <param name="session">the Session to use for creating a NMS Message
        /// </param>
        /// <returns> the NMS Message
        /// </returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public IMessage ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert is IMessage)
            {
                return (IMessage) objectToConvert;
            }
            else if (objectToConvert is string)
            {
                return CreateMessageForString((string) objectToConvert, session);
            }
            else if (objectToConvert is sbyte[])
            {
                return CreateMessageForByteArray((byte[]) objectToConvert, session);
            }
            else if (objectToConvert is IDictionary)
            {
                return CreateMessageForMap((IDictionary) objectToConvert, session);
            }
            else if (objectToConvert != null && objectToConvert.GetType().IsSerializable)
            {
                return
                    CreateMessageForSerializable(objectToConvert, session);
            }
            else
            {
                throw new MessageConversionException("Cannot convert object [" + objectToConvert + "] to NMS message");
            }
        }

        /// <summary> Convert from a NMS Message to a .NET object.</summary>
        /// <param name="messageToConvert">the message to convert
        /// </param>
        /// <returns> the converted .NET object
        /// </returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public object FromMessage(IMessage messageToConvert)
        {
            if (messageToConvert is ITextMessage)
            {
                return ExtractStringFromMessage((ITextMessage) messageToConvert);
            }
            else if (messageToConvert is IBytesMessage)
            {
                return ExtractByteArrayFromMessage((IBytesMessage) messageToConvert);
            }
            else if (messageToConvert is IMapMessage)
            {
                return ExtractMapFromMessage((IMapMessage) messageToConvert);
            }
            else if (messageToConvert is IObjectMessage)
            {
                return ExtractSerializableFromMessage((IObjectMessage) messageToConvert);
            }
            else
            {
                return messageToConvert;
            }
        }

        #region To Converter Methods

        /// <summary> Create a NMS ITextMessage for the given String.</summary>
        /// <param name="text">the String to convert
        /// </param>
        /// <param name="session">current NMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual ITextMessage CreateMessageForString(string text, ISession session)
        {
            return session.CreateTextMessage((text));
        }

        /// <summary> Create a NMS IBytesMessage for the given byte array.</summary>
        /// <param name="bytes">the byyte array to convert
        /// </param>
        /// <param name="session">current NMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual IBytesMessage CreateMessageForByteArray(byte[] bytes, ISession session)
        {
            IBytesMessage message = session.CreateBytesMessage();
            message.Content = bytes;
            return message;
        }

        /// <summary> Create a NMS IMapMessage for the given Map.</summary>
        /// <param name="map">the Map to convert
        /// </param>
        /// <param name="session">current NMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual IMapMessage CreateMessageForMap(IDictionary map, ISession session)
        {
            IMapMessage mapMessage = session.CreateMapMessage();
            foreach (DictionaryEntry entry in map)
            {
                if (!(entry.Key is string))
                {
                    //UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Class.getName' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
                    throw new MessageConversionException("Cannot convert non-String key of type [" +
                                                         (entry.Key != null ? entry.Key.GetType().FullName : null) +
                                                         "] to IMapMessage entry");
                }
                mapMessage.Body[entry.Key.ToString()] = entry.Value;
            }
            return mapMessage;
        }

        /// <summary> Create a NMS IObjectMessage for the given Serializable object.</summary>
        /// <param name="objectToSend">the Serializable object to convert
        /// </param>
        /// <param name="session">current NMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual IObjectMessage CreateMessageForSerializable(
            object objectToSend, ISession session)
        {
            return session.CreateObjectMessage(objectToSend);
        }

        #endregion

        #region From Converter Mehtods

        /// <summary> Extract a String from the given ITextMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting String
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual string ExtractStringFromMessage(ITextMessage message)
        {
            return message.Text;
        }

        /// <summary> Extract a byte array from the given IBytesMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting byte array
        /// </returns>
        /// <throws>  NMSException if thrown by NMS methods </throws>
        protected virtual byte[] ExtractByteArrayFromMessage(IBytesMessage message)
        {
			return message.Content;
        }

        /// <summary> Extract a IDictionary from the given IMapMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting Map
        /// </returns>
        /// <throws>NMSException if thrown by NMS methods </throws>
        protected virtual IDictionary ExtractMapFromMessage(IMapMessage message)
        {
            IDictionary dictionary = new Hashtable();
            IEnumerator e = message.Body.Keys.GetEnumerator();
            while (e.MoveNext())
            {
                String key = (String) e.Current;
                dictionary.Add(key, message.Body[key]);
            }
            return dictionary;
        }

        /// <summary>
        /// Extracts the serializable object from the given object message.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The resulting serializable object.</returns>
        protected virtual object ExtractSerializableFromMessage(
            IObjectMessage message)
        {
            return message.Body;
        }

        #endregion
    }
}

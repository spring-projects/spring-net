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
using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Support.Converter
{
    /// <summary> A simple message converter that can handle TextMessages, BytesMessages,
    /// MapMessages, and ObjectMessages. Used as default by EmsTemplate, for
    /// <code>ConvertAndSend</code> and <code>ReceiveAndConvert</code> operations.
    ///
    /// <p>Converts a String to a EMS TextMessage, a byte array to a EMS BytesMessage,
    /// a Map to a EMS MapMessage, and a Serializable object to a EMS ObjectMessage
    /// (or vice versa).</p>
    ///
    ///
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class SimpleMessageConverter : IMessageConverter
    {
        /// <summary> Convert a .NET object to a EMS Message using the supplied session
        /// to create the message object.
        /// </summary>
        /// <param name="objectToConvert">the object to convert
        /// </param>
        /// <param name="session">the Session to use for creating a EMS Message
        /// </param>
        /// <returns> the EMS Message
        /// </returns>
        /// <throws>EMSException if thrown by EMS API methods </throws>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public Message ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert is Message)
            {
                return (Message) objectToConvert;
            }
            if (objectToConvert is string)
            {
                return CreateMessageForString((string) objectToConvert, session);
            }
            if (objectToConvert is sbyte[])
            {
                return CreateMessageForByteArray((byte[]) objectToConvert, session);
            }
            if (objectToConvert is IDictionary)
            {
                return CreateMessageForMap((IDictionary) objectToConvert, session);
            }
            if (objectToConvert != null && objectToConvert.GetType().IsSerializable)
            {
                return CreateMessageForSerializable(objectToConvert, session);
            }
            throw new MessageConversionException("Cannot convert object [" + objectToConvert + "] to EMS message");
        }

        /// <summary> Convert from a EMS Message to a .NET object.</summary>
        /// <param name="messageToConvert">the message to convert
        /// </param>
        /// <returns> the converted .NET object
        /// </returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public object FromMessage(Message messageToConvert)
        {
            if (messageToConvert is TextMessage)
            {
                return ExtractStringFromMessage((TextMessage) messageToConvert);
            }
            if (messageToConvert is BytesMessage)
            {
                return ExtractByteArrayFromMessage((BytesMessage) messageToConvert);
            }
            if (messageToConvert is MapMessage)
            {
                return ExtractMapFromMessage((MapMessage) messageToConvert);
            }
            if (messageToConvert is ObjectMessage)
            {
                return ExtractSerializableFromMessage((ObjectMessage) messageToConvert);
            }
            return messageToConvert;
        }

        #region To Converter Methods

        /// <summary> Create a EMS TextMessage for the given String.</summary>
        /// <param name="text">the String to convert
        /// </param>
        /// <param name="session">current EMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual TextMessage CreateMessageForString(string text, ISession session)
        {
            return session.CreateTextMessage((text));
        }

        /// <summary> Create a EMS BytesMessage for the given byte array.</summary>
        /// <param name="bytes">the byyte array to convert
        /// </param>
        /// <param name="session">current EMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual BytesMessage CreateMessageForByteArray(byte[] bytes, ISession session)
        {
            BytesMessage message = session.CreateBytesMessage();
            message.WriteBytes(bytes);
            return message;
        }

        /// <summary> Create a EMS MapMessage for the given Map.</summary>
        /// <param name="map">the Map to convert
        /// </param>
        /// <param name="session">current EMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual MapMessage CreateMessageForMap(IDictionary map, ISession session)
        {
            MapMessage mapMessage = session.CreateMapMessage();
            foreach (DictionaryEntry entry in map)
            {
                if (!(entry.Key is string))
                {
                    throw new MessageConversionException("Cannot convert non-String key of type [" +
                                                         (entry.Key != null ? entry.Key.GetType().FullName : null) +
                                                         "] to MapMessage entry");
                }
                mapMessage.SetObject(entry.Key.ToString(), entry.Value);
            }
            return mapMessage;
        }

        /// <summary> Create a EMS ObjectMessage for the given Serializable object.</summary>
        /// <param name="objectToSend">the Serializable object to convert
        /// </param>
        /// <param name="session">current EMS session
        /// </param>
        /// <returns> the resulting message
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual ObjectMessage CreateMessageForSerializable(
            object objectToSend, ISession session)
        {
            return session.CreateObjectMessage(objectToSend);
        }

        #endregion

        #region From Converter Mehtods

        /// <summary> Extract a String from the given TextMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting String
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual string ExtractStringFromMessage(TextMessage message)
        {
            return message.Text;
        }

        /// <summary> Extract a byte array from the given BytesMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting byte array
        /// </returns>
        /// <throws>  EMSException if thrown by EMS methods </throws>
        protected virtual byte[] ExtractByteArrayFromMessage(BytesMessage message)
        {
            byte[] bytes = new byte[(int)message.BodyLength];
            message.ReadBytes(bytes);
            return bytes;
        }

        /// <summary> Extract a IDictionary from the given MapMessage.</summary>
        /// <param name="message">the message to convert
        /// </param>
        /// <returns> the resulting Map
        /// </returns>
        /// <throws>EMSException if thrown by EMS methods </throws>
        protected virtual IDictionary ExtractMapFromMessage(MapMessage message)
        {
            IDictionary dictionary = new Hashtable();
            IEnumerator e = message.MapNames;
            while (e.MoveNext())
            {
                String key = (String)e.Current;
                dictionary.Add(key, message.GetObject(key));
            }

            return dictionary;
        }

        /// <summary>
        /// Extracts the serializable object from the given object message.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The resulting serializable object.</returns>
        protected virtual object ExtractSerializableFromMessage(
            ObjectMessage message)
        {
            return message.TheObject;
        }

        #endregion
    }
}

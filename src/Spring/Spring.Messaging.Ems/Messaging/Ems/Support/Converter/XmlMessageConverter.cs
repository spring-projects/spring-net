using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Support.Converter
{
    /// <summary>
    /// Convert an object via XML serialization for sending via an ITextMessage
    /// </summary>
    /// <author>Mark Pollack</author>
    public class XmlMessageConverter : IMessageConverter
    {
        private IMessageConverter defaultMessageConverter = new SimpleMessageConverter();


        private ITypeMapper typeMapper;


        /// <summary>
        /// Sets the type mapper.
        /// </summary>
        /// <value>The type mapper.</value>
        public ITypeMapper TypeMapper
        {
            set { typeMapper = value; }
        }

        /// <summary>
        /// Convert a .NET object to a NMS Message using the supplied session
        /// to create the message object.
        /// </summary>
        /// <param name="objectToConvert">the object to convert</param>
        /// <param name="session">the Session to use for creating a NMS Message</param>
        /// <returns>the NMS Message</returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public Message ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert == null)
            {
                throw new MessageConversionException("Can't convert null object");
            }
            try
            {
                if (objectToConvert.GetType().Equals(typeof(string)) ||
                    typeof(IDictionary).IsAssignableFrom(objectToConvert.GetType()) ||
                    objectToConvert.GetType().Equals(typeof(Byte[])))
                {
                    return defaultMessageConverter.ToMessage(objectToConvert, session);
                }
                else
                {
                    string xmlString = GetXmlString(objectToConvert);
                    Message msg = session.CreateTextMessage(xmlString);
                    msg.SetStringProperty(typeMapper.TypeIdFieldName, typeMapper.FromType(objectToConvert.GetType()));
                    return msg;
                }
            }
            catch (Exception e)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), e);
            }
        }

        /// <summary>
        /// Gets the XML string for an object
        /// </summary>
        /// <param name="objectToConvert">The object to convert.</param>
        /// <returns>XML string</returns>
        protected virtual string GetXmlString(object objectToConvert)
        {
            string xmlString;
            XmlTextWriter xmlTextWriter = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                XmlSerializer xs = new XmlSerializer(objectToConvert.GetType());
                xs.Serialize(xmlTextWriter, objectToConvert);
                xmlString = UTF8ByteArrayToString(((MemoryStream)xmlTextWriter.BaseStream).ToArray());
            }
            catch (Exception e)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), e);
            }
            finally
            {
                if (memoryStream != null) memoryStream.Close();
            }
            return xmlString;
        }

        /// <summary>
        /// Convert from a NMS Message to a .NET object.
        /// </summary>
        /// <param name="messageToConvert">the message to convert</param>
        /// <returns>the converted .NET object</returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public object FromMessage(Message messageToConvert)
        {
            if (messageToConvert == null)
            {
                throw new MessageConversionException("Can't convert null message");
            }
            try
            {
                string converterId = messageToConvert.GetStringProperty(typeMapper.TypeIdFieldName);
                if (converterId == null)
                {
                    return defaultMessageConverter.FromMessage(messageToConvert);
                }
                else
                {
                    TextMessage textMessage = messageToConvert as TextMessage;
                    if (textMessage == null)
                    {
                        throw new MessageConversionException("Can't convert message of type " +
                                                             messageToConvert.GetType());
                    }

                    using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(textMessage.Text)))
                    {

                        XmlSerializer xs = new XmlSerializer(GetTargetType(textMessage));
                        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                        return xs.Deserialize(memoryStream);
                    }
                }
            }
            catch (Exception e)
            {
                throw new MessageConversionException("Can't convert message of type " + messageToConvert.GetType(), e);
            }
        }

        /// <summary>
        /// Gets the type of the target given the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Type of the target</returns>
        protected virtual Type GetTargetType(TextMessage message)
        {
            return typeMapper.ToType(message.GetStringProperty(typeMapper.TypeIdFieldName));
        }


        /// <summary>
        /// Converts a byte array to a UTF8 string.
        /// </summary>
        /// <param name="characters">The characters.</param>
        /// <returns>UTF8 string</returns>
        protected virtual String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            String constructedString = encoding.GetString(characters);

            return (constructedString);
        }


        /// <summary>
        /// Converts a UTF8 string to a byte array
        /// </summary>
        /// <param name="xmlString">The p XML string.</param>
        /// <returns></returns>
        protected virtual Byte[] StringToUTF8ByteArray(String xmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            Byte[] byteArray = encoding.GetBytes(xmlString);

            return byteArray;
        }
    }
}
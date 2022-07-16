using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Apache.NMS;

namespace Spring.Messaging.Nms.Support.Converter
{
    /// <summary>
    /// Convert an object via XML serialization for sending via an ITextMessage
    /// </summary>
    /// <author>Mark Pollack</author>
    public class XmlMessageConverter : IMessageConverter
    {
        private IMessageConverter defaultMessageConverter = new SimpleMessageConverter();


        private ITypeMapper typeMapper = new TypeMapper();

        private bool encoderShouldEmitUTF8Identifier = false;

        private bool throwOnInvalidBytes = true;


        /// <summary>
        /// Sets the type mapper.
        /// </summary>
        /// <value>The type mapper.</value>
        public ITypeMapper TypeMapper
        {
            set { typeMapper = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether encoder should emit UTF8 byte order mark.  Default is false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to specify that a Unicode byte order mark is provided; otherwise, <c>false</c>.
        /// </value>
        public bool EncoderShouldEmitUtf8Identifier
        {
            get { return encoderShouldEmitUTF8Identifier; }
            set { encoderShouldEmitUTF8Identifier = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception on invalid bytes.  Default is true. </summary>
        /// <value>
        /// 	<c>true</c> to specify that an exception be thrown when an invalid encoding is detected; otherwise, <c>false</c>.
        /// </value>
        public bool ThrowOnInvalidBytes
        {
            get { return throwOnInvalidBytes; }
            set { throwOnInvalidBytes = value; }
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
        public IMessage ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert == null)
            {
                throw new MessageConversionException("Can't convert null object");
            }
            try
            {
                if (objectToConvert.GetType().Equals(typeof (string)) ||
                    typeof (IDictionary).IsAssignableFrom(objectToConvert.GetType()) ||
                    objectToConvert.GetType().Equals(typeof (Byte[])))
                {
                    return defaultMessageConverter.ToMessage(objectToConvert, session);
                }
                string xmlString = GetXmlString(objectToConvert);
                IMessage msg = session.CreateTextMessage(xmlString);
                msg.Properties.SetString(typeMapper.TypeIdFieldName, typeMapper.FromType(objectToConvert.GetType()));
                return msg;
            } catch (Exception e)
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
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(EncoderShouldEmitUtf8Identifier)))
                    {
                        XmlSerializer xs = new XmlSerializer(objectToConvert.GetType());
                        xs.Serialize(xmlTextWriter, objectToConvert);
                        xmlString = UTF8ByteArrayToString(((MemoryStream)xmlTextWriter.BaseStream).ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), e);
            }
            return xmlString;
        }

        /// <summary>
        /// Convert from a NMS Message to a .NET object.
        /// </summary>
        /// <param name="messageToConvert">the message to convert</param>
        /// <returns>the converted .NET object</returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public object FromMessage(IMessage messageToConvert)
        {
            if (messageToConvert == null)
            {
                throw new MessageConversionException("Can't convert null message");
            }
            try
            {
                string converterId = messageToConvert.Properties.GetString(typeMapper.TypeIdFieldName);
                if (converterId == null)
                {
                    return defaultMessageConverter.FromMessage(messageToConvert);
                }
                ITextMessage textMessage = messageToConvert as ITextMessage;
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
            } catch (Exception e)
            {
                throw new MessageConversionException("Can't convert message of type " + messageToConvert.GetType(), e);
            }
        }

        /// <summary>
        /// Gets the type of the target given the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Type of the target</returns>
        protected virtual Type GetTargetType(ITextMessage message)
        {
            return typeMapper.ToType(message.Properties.GetString(typeMapper.TypeIdFieldName));
        }


        /// <summary>
        /// Converts a byte array to a UTF8 string.
        /// </summary>
        /// <param name="characters">The characters.</param>
        /// <returns>UTF8 string</returns>
        protected virtual String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding(EncoderShouldEmitUtf8Identifier, ThrowOnInvalidBytes);

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

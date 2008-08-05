using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Apache.NMS;
using Spring.Messaging.Nms.Support.Converter;

namespace Spring.NmsQuickStart.Common.Converters
{
    public class XmlMessageConverter : IMessageConverter
    {
        private IMessageConverter defaultMessageConverter = new SimpleMessageConverter();


        private ITypeMapper typeMapper;


        public ITypeMapper TypeMapper
        {
            set { typeMapper = value; }
        }

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
                else
                {
                    string xmlString = GetXmlString(objectToConvert);
                    IMessage msg = session.CreateTextMessage(xmlString);
                    msg.Properties.SetString(typeMapper.TypeIdFieldName, typeMapper.FromType(objectToConvert.GetType()));
                    return msg;
                }
            } catch (Exception e)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), e);
            }
        }

        private string GetXmlString(object objectToConvert)
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
                xmlString = UTF8ByteArrayToString(((MemoryStream) xmlTextWriter.BaseStream).ToArray());
            } catch (Exception e)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), e);           
            } finally
            {
                if (memoryStream != null) memoryStream.Close();
                if (xmlTextWriter != null) xmlTextWriter.Close();
            }
            return xmlString;
        }

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
                else
                {
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
                }
            } catch (Exception e)
            {
                throw new MessageConversionException("Can't convert message of type " + messageToConvert.GetType(), e);
            }
        }

        private Type GetTargetType(ITextMessage message)
        {
            return typeMapper.ToType(message.Properties.GetString(typeMapper.TypeIdFieldName));
        }


        private String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            String constructedString = encoding.GetString(characters);

            return (constructedString);
        }


        private Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            Byte[] byteArray = encoding.GetBytes(pXmlString);

            return byteArray;
        }
    }
}


using System;
using System.Collections;
using Apache.NMS;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Objects.Factory;

namespace Spring.NmsQuickStart.Common.Converters
{
    public class MultiMessageConverter : IMessageConverter, IInitializingObject
    {
        private IMessageConverter defaultMessageConverter = new SimpleMessageConverter();

        private IDictionary typeConverterMapping = new Hashtable();

        private IDictionary nameConverterMapping = new Hashtable();

        private IList namedMessageConverters = new ArrayList();


        public MultiMessageConverter()
        {

        }


        public IList NamedMessageConverters
        {
            set { namedMessageConverters = value; }
        }

        private string converterIdFieldName = "__ConverterId__";

        public IMessage ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert == null)
            {
                throw new MessageConversionException("Can't convert null object");
            }
            if (objectToConvert.GetType().Equals(typeof(string)) ||
                typeof(IDictionary).IsAssignableFrom(objectToConvert.GetType()) ||
                objectToConvert.GetType().Equals(typeof(Byte[])))
            {
                return defaultMessageConverter.ToMessage(objectToConvert, session);
            }
            else
            {
                INamedMessageConverter converter = GetConverterForType(objectToConvert.GetType());
                if (converter != null)
                {
                    IMessage msg = converter.ToMessage(objectToConvert, session);
                    msg.Properties.SetString(converterIdFieldName, converter.Name);
                    return msg;
                }
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType());
            }
        }

        private INamedMessageConverter GetConverterForType(Type typeOfObjectToConvert)
        {
            if (typeConverterMapping.Contains(typeOfObjectToConvert))
            {
                return typeConverterMapping[typeOfObjectToConvert] as INamedMessageConverter;
            }
            return null;
        }

        public object FromMessage(IMessage messageToConvert)
        {
            if (messageToConvert == null)
            {
                throw new MessageConversionException("Can't convert null message");
            }
            string converterId = messageToConvert.Properties.GetString(converterIdFieldName);
            if (converterId == null)
            {
                return defaultMessageConverter.FromMessage(messageToConvert);
            }
            else
            {
                IMessageConverter converter = GetConverterForId(converterId);
                if (converter != null)
                {
                    return converter.FromMessage(messageToConvert);
                }
                throw new MessageConversionException("Can't convert message with ConverterId = " + converterId + ".  Message = " + messageToConvert);
            
            }
        }

        private IMessageConverter GetConverterForId(string converterName)
        {
            if (nameConverterMapping.Contains(converterName))
            {
                return nameConverterMapping[converterName] as IMessageConverter;
            }
            return null;
        }

        public void AfterPropertiesSet()
        {
            foreach (INamedMessageConverter namedMessageConverter in namedMessageConverters)
            {
                nameConverterMapping.Add(namedMessageConverter.Name, namedMessageConverter);
                typeConverterMapping.Add(namedMessageConverter.TargetType, namedMessageConverter);
            }
        }
    }
}
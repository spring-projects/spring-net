

using System;
using Apache.NMS;
using Spring.Objects.Factory;

namespace Spring.NmsQuickStart.Common.Converters
{
    public abstract class AbstractNamedMessageConverter : INamedMessageConverter, IObjectNameAware
    {
        private string objectName;


        public string Name
        {
            get { return objectName; }
            set { objectName = value; }
        }

        public abstract Type TargetType { get; }

        public string ObjectName
        {
            set { objectName = value; }
        }

        public abstract IMessage ToMessage(object objectToConvert, ISession session);
        public abstract object FromMessage(IMessage messageToConvert);
    }
}
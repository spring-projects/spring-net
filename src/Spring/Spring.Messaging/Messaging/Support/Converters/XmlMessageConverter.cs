#region License

/*
 * Copyright 2002-2008 the original author or authors.
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


using System;
using System.Messaging;
using Spring.Util;

namespace Spring.Messaging.Support.Converters
{
    public class XmlMessageConverter : IMessageConverter
    {
        private XmlMessageFormatter messageFormatter;

        public XmlMessageConverter()
        {
            messageFormatter = new XmlMessageFormatter();
        }

        public XmlMessageConverter(XmlMessageFormatter messageFormatter)
        {
            this.messageFormatter = messageFormatter;
        }

        public Type[] TargetTypes
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "TargetTypes");
                messageFormatter.TargetTypes = value;
            }
            get { return messageFormatter.TargetTypes; }
        }

        public string[] TargetTypeNames
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "TargetTypeNames");
                messageFormatter.TargetTypeNames = value;
            }
            get { return messageFormatter.TargetTypeNames; }
        }

        #region IMessageConverter Members

        public Message ToMessage(object obj)
        {
            Message m = new Message();
            m.Body = obj;
            m.Formatter = messageFormatter;
            return m;
        }

        public object FromMessage(Message message)
        {
            message.Formatter = messageFormatter;
            return message.Body;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            XmlMessageConverter mc = new XmlMessageConverter(messageFormatter.Clone() as XmlMessageFormatter);
            return mc;
        }

        #endregion
    }
}
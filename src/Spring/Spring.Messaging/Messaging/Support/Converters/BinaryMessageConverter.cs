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
using System.Runtime.Serialization.Formatters;

namespace Spring.Messaging.Support.Converters
{
    public class BinaryMessageConverter : IMessageConverter
    {
        private BinaryMessageFormatter binaryMessageFormatter;

        private FormatterTypeStyle typeFormat;
        private FormatterAssemblyStyle topObjectFormat;
        public BinaryMessageConverter()
        {
            binaryMessageFormatter = new BinaryMessageFormatter();
        }


        public BinaryMessageConverter(BinaryMessageFormatter binaryMessageFormatter)
        {
            this.binaryMessageFormatter = binaryMessageFormatter;
        }

        public FormatterTypeStyle TypeFormat
        {
            get { return typeFormat; }
            set { typeFormat = value; }
        }

        public FormatterAssemblyStyle TopObjectFormat
        {
            get { return topObjectFormat; }
            set { topObjectFormat = value; }
        }

        #region IMessageConverter Members

        public Message ToMessage(object obj)
        {
            Message m = new Message();
            m.Body = obj;
            m.Formatter = binaryMessageFormatter;
            return m;
        }

        public object FromMessage(Message message)
        {
            message.Formatter = binaryMessageFormatter;
            return message.Body;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            BinaryMessageConverter mc = new BinaryMessageConverter(binaryMessageFormatter.Clone() as BinaryMessageFormatter);
            return mc;
        }

        #endregion
    }
}
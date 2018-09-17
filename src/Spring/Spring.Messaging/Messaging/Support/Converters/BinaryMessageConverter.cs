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

using System.Runtime.Serialization.Formatters;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Support.Converters
{
    /// <summary>
    /// An <see cref="IMessageConverter"/> implementation that delegates to an instance of
    /// <see cref="BinaryMessageFormatter"/> to convert messages.  
    /// </summary>
    /// <author>Mark Pollack</author>
    public class BinaryMessageConverter : IMessageConverter
    {
        private readonly BinaryMessageFormatter binaryMessageFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryMessageConverter"/> class.
        /// </summary>
        public BinaryMessageConverter()
        {
            binaryMessageFormatter = new BinaryMessageFormatter();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryMessageConverter"/> class.
        /// </summary>
        /// <param name="binaryMessageFormatter">The binary message formatter.</param>
        public BinaryMessageConverter(BinaryMessageFormatter binaryMessageFormatter)
        {
            this.binaryMessageFormatter = binaryMessageFormatter;            
        }

        /// <summary>
        /// Gets or sets the type format used in the <see cref="BinaryMessageFormatter"/>
        /// </summary>
        /// <value>The type format.</value>
        public FormatterTypeStyle TypeFormat
        {
            get
            {
                return binaryMessageFormatter.TypeFormat;
            }
            set { 
                binaryMessageFormatter.TypeFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the top object format used in the <see cref="BinaryMessageFormatter"/>
        /// </summary>
        /// <value>The top object format.</value>
        public FormatterAssemblyStyle TopObjectFormat
        {
            get { return binaryMessageFormatter.TopObjectFormat; }
            set
            {                
                binaryMessageFormatter.TopObjectFormat = value;
            }
        }

        #region IMessageConverter Members

        /// <summary>
        /// Convert the given object to a Message using the <see cref="BinaryMessageFormatter"/>
        /// </summary>
        /// <param name="obj">The object to send.</param>
        /// <returns>Message to send</returns>
        public Message ToMessage(object obj)
        {
            Message m = new Message();
            m.Body = obj;
            m.Formatter = binaryMessageFormatter;
            return m;
        }

        /// <summary>
        /// Convert the given message to a object using the <see cref="BinaryMessageFormatter"/>
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the object</returns>
        public object FromMessage(Message message)
        {
            message.Formatter = binaryMessageFormatter;
            return message.Body;
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
            //takes into account TypeFormat and TypeObjectFormat
            BinaryMessageConverter mc = new BinaryMessageConverter(binaryMessageFormatter.Clone() as BinaryMessageFormatter);
            return mc;
        }

        #endregion
    }
}
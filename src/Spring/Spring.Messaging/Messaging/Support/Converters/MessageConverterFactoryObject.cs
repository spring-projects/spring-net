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

using Spring.Objects.Factory.Config;

namespace Spring.Messaging.Support.Converters
{
    /// <summary>
    /// Internal class to that users can specify a delegate function to register with the application context that
    /// will create a IMessageConverter instance easily at runtime.
    /// </summary>
    /// <author>Mark Pollack</author>
    internal class MessageConverterFactoryObject : IConfigurableFactoryObject
    {
        private IObjectDefinition productTemplate;

        private MessageConverterCreatorDelegate messageConverterCreatorDelegate;


        public MessageConverterCreatorDelegate MessageConverterCreatorDelegate
        {
            get { return messageConverterCreatorDelegate; }
            set { messageConverterCreatorDelegate = value; }
        }

        #region IConfigurableFactoryObject Members


        /// <summary>
        /// Gets the template object definition that should be used
        /// to configure the instance of the object managed by this factory.
        /// </summary>
        /// <value>The object definition to configure the factory's product</value>
        public IObjectDefinition ProductTemplate
        {
            get { return productTemplate; }
            set { productTemplate = value; }
        }

        #endregion

        #region IFactoryObject Members

        public object GetObject()
        {
            return MessageConverterCreatorDelegate();
        }

        public Type ObjectType
        {
            get { return typeof(IMessageConverter); }
        }

        public bool IsSingleton
        {
            get { return false; }
        }

        #endregion
    }
}

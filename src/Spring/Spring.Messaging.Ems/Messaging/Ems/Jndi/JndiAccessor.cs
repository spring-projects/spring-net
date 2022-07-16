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

using System.Collections;
using Common.Logging;
using Spring.Objects.Factory;

namespace Spring.Messaging.Ems.Jndi
{
    /// <summary>
    /// Convenient superclass to access JndiProperties, JndiContextType or alternatively set the ILookupContext directly.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public class JndiAccessor : IInitializingObject
    {
        private ILookupContext _jndiLookupContext;

        private Hashtable jndiProperties = new Hashtable();

        private JndiContextType contextType = JndiContextType.JMS;

        protected ILog logger;

        LookupContextFactory contextFactoryObject = new LookupContextFactory();

        /// <summary>
        /// Gets or sets the lookup context.
        /// </summary>
        /// <value>The lookup context.</value>
        public ILookupContext JndiLookupContext
        {
            get { return _jndiLookupContext; }
            set { _jndiLookupContext = value; }
        }

        /// <summary>
        /// Gets or sets the JNDI environment properties.
        /// </summary>
        /// <value>The jndi properties.</value>
        public IDictionary JndiProperties
        {
            get
            {
                return jndiProperties;
            }
            set
            {
                jndiProperties = new Hashtable(value);
            }
        }

        /// <summary>
        /// Gets or sets the type of the jndi context.  The default is JndiContextType.JMS
        /// </summary>
        /// <value>The type of the jndi context.</value>
        public JndiContextType JndiContextType
        {
            get { return this.contextType;  }
            set
            {
                this.contextType = value;
            }
        }


        /// <summary>
        /// Create the JndiLookupContext if it has not been explicitly set.
        /// </summary>
        public virtual void AfterPropertiesSet()
        {
            if (_jndiLookupContext == null)
            {
                if (JndiContextType == JndiContextType.JMS)
                {
                    this._jndiLookupContext = contextFactoryObject.CreateContext(LookupContextFactory.TIBJMS_NAMING_CONTEXT,
                                                                            jndiProperties);
                }
                else
                {
                    this._jndiLookupContext = contextFactoryObject.CreateContext(LookupContextFactory.LDAP_CONTEXT,
                                                                            jndiProperties);
                }
            }
        }
    }
}

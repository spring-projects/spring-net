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

using Spring.Util;

namespace Spring.Messaging.Ems.Jndi
{
    /// <summary>
    /// Convenient superclass for JNDI-based service locators,
    /// providing configurable lookup of a specific JNDI resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exposes a JndiName property.
    /// </para>
    /// <para>Subclasses may invoke the <code>Lookup</code> method whenever it is appropriate.
    /// Some classes might do this on initialization, while others might do it
    /// on demand. The latter strategy is more flexible in that it allows for
    /// initialization of the locator before the JNDI object is available.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class JndiObjectLocator : JndiLocatorSupport
    {
        private string jndiName;

        private Type expectedType;

        /// <summary>
        /// Gets or sets the Jndi name to lookup.
        /// </summary>
        /// <value>The name of the jndi.</value>
        public string JndiName
        {
            get { return jndiName; }
            set { jndiName = value; }
        }

        /// <summary>
        /// Gets or sets the type that the located JNDI object is supposed
        /// to be assignable to, if any.
        /// </summary>
        /// <value>The expected type.</value>
        public Type ExpectedType
        {
            get { return expectedType; }
            set { expectedType = value; }
        }

        #region Implementation of IInitializingObject

        /// <summary>
        /// Ensure that the JndiName property is set and create the TIBCO EMS ILookupContext instance.
        /// </summary>
        public override void AfterPropertiesSet()
        {
            base.AfterPropertiesSet();
            if (!StringUtils.HasLength(JndiName))
            {
                throw new ArgumentException("Property 'JndiName' is required");
            }
        }

        /// <summary>
        /// Lookups this instance using the JndiName and ExpectedType properties
        /// </summary>
        /// <returns>The object retrieved from Jndi</returns>
        protected virtual object Lookup() {
		    return Lookup(JndiName, ExpectedType);
	    }

        #endregion
    }
}

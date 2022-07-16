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

using Spring.Objects.Factory;

namespace Spring.Messaging.Ems.Jndi
{
    /// <summary>
    /// A Spring FactoryObject that returns TIBCO.EMS.ILookupContext.  Use the returned
    /// ILookupContext to do you lookups at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Important properties to set are JndiProperties and JndiContexType.  JndiContextType is set to
    /// LookupContextFactory.TIBJMS_NAMING_CONT by default.
    /// </para>
    /// <para>To lookup objects at startup time and cache their values, as well as provide a 
    /// default value if lookup fail, <see cref="JndiLookupFactoryObject"/>
    /// </para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class LookupContextFactoryObject : JndiLocatorSupport, IFactoryObject
    {
        #region Implementation of IFactoryObject

        /// <summary>
        /// Returns the TIBCO.EMS.ILookupContext
        /// </summary>
        /// <returns>TIBCO.EMS.ILookupContext</returns>
        public object GetObject()
        {
            return this.JndiLookupContext;
        }

        /// <summary>
        /// Return typeof(TIBCO.EMS.ILookupContext)
        /// </summary>
        public Type ObjectType
        {
            get { return typeof (ILookupContext);  }
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsSingleton
        {
            get { return true; }
        }

        #endregion

    }
}

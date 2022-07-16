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

using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Util;
using Common.Logging;

namespace Spring.Messaging.Ems.Jndi
{
    public class JndiLookupFactoryObject : JndiObjectLocator, IConfigurableFactoryObject
    {
        static JndiLookupFactoryObject()
        {
             TypeRegistry.RegisterType("LookupContext", typeof(LookupContext));
             TypeRegistry.RegisterType("JndiContextType", typeof(JndiContextType));
        }

        private object defaultObject;
        private Object jndiObject;
        private IObjectDefinition productTemplate;

        public JndiLookupFactoryObject()
        {
            this.logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Sets the default object to fall back to if the JNDI lookup fails.
        /// Default is none.
        /// </summary>
        /// <remarks>
        /// <para>This can be an arbitrary bean reference or literal value.
        /// It is typically used for literal values in scenarios where the JNDI environment
        /// might define specific config settings but those are not required to be present.
        /// </para>
        /// </remarks>
        /// <value>The default object to use when lookup fails.</value>
        public object DefaultObject
        {
            set { defaultObject = value; }
        }

        #region Implementation of IFactoryObject

        /// <summary>
        /// Return the Jndi object
        /// </summary>
        /// <returns>The Jndi  object</returns>
        public object GetObject()
        {
            return this.jndiObject;
        }


        /// <summary>
        /// Return type of object retrieved from Jndi or the expected type if the Jndi retrieval
        /// did not succeed.
        /// </summary>
        /// <value>Return value of retrieved object</value>
        public Type ObjectType
        {
            get
            {
                if (this.jndiObject != null)
                {
                    return this.jndiObject.GetType();
                }
                return ExpectedType;
            }
        }


        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsSingleton
        {
            get { return true; }
        }

        #endregion

        public override void AfterPropertiesSet()
        {
            base.AfterPropertiesSet();
            TypeRegistry.RegisterType("LookupContext", typeof(LookupContext));
            if (this.defaultObject != null && ExpectedType != null &&
                    !ObjectUtils.IsAssignable(ExpectedType, this.defaultObject))
            {
                throw new ArgumentException("Default object [" + this.defaultObject +
                                            "] of type [" + this.defaultObject.GetType().Name +
                                            "] is not of expected type [" + ExpectedType.Name + "]");
            }
            // Locate specified JNDI object.
            this.jndiObject = LookupWithFallback();
        }

        protected virtual object LookupWithFallback()
        {
            try
            {
                return Lookup();
            }
            catch (TypeMismatchNamingException)
            {
                // Always let TypeMismatchNamingException through -
                // we don't want to fall back to the defaultObject in this case.
                throw;
            }
            catch (NamingException ex)
            {
                if (this.defaultObject != null)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("JNDI lookup failed - returning specified default object instead", ex);
                    }
                    else if (logger.IsInfoEnabled)
                    {
                        logger.Info("JNDI lookup failed - returning specified default object instead: " + ex);
                    }
                    return this.defaultObject;
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the template object definition that should be used
        /// to configure the instance of the object managed by this factory.
        /// </summary>
        /// <value></value>
        public IObjectDefinition ProductTemplate
        {
            get { return productTemplate; }
            set { productTemplate = value; }
        }
    }
}

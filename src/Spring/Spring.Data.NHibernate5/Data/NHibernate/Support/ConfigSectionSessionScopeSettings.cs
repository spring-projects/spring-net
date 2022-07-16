/*
 * Copyright © 2002-2011 the original author or authors.
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

using NHibernate;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Holds the references and configuration settings for a <see cref="SessionScope"/> instance. 
    /// References are resolved by looking up the given object names in the root <see cref="IApplicationContext"/> obtained by <see cref="ContextRegistry.GetContext()"/>.
    /// </summary>
    public class ConfigSectionSessionScopeSettings 
        : SessionScopeSettings
    {
        /// <summary>
        /// The default session factory name to use when retrieving the Hibernate session factory from
        /// the root context.
        /// </summary>
        public static readonly string DEFAULT_SESSION_FACTORY_OBJECT_NAME = "SessionFactory";

        private readonly string sessionFactoryObjectName = DEFAULT_SESSION_FACTORY_OBJECT_NAME;
        private readonly string entityInterceptorObjectName = null;

        /// <summary>
        /// Initializes a new <see cref="ConfigSectionSessionScopeSettings"/> instance.
        /// </summary>
        /// <param name="ownerType">The type, who's name will be used to prefix setting variables with</param>
        public ConfigSectionSessionScopeSettings(Type ownerType)
            : this(ownerType, "appSettings")
        {
            // noop
        }

        /// <summary>
        /// Initializes a new <see cref="ConfigSectionSessionScopeSettings"/> instance.
        /// </summary>
        /// <param name="ownerType">The type, who's name will be used to prefix setting variables with</param>
        /// <param name="sectionName">The configuration section to read setting variables from.</param>
        public ConfigSectionSessionScopeSettings(Type ownerType, string sectionName)
            : this(ownerType, new ConfigSectionVariableSource(sectionName))
        {
            // noop
        }

        /// <summary>
        /// Initializes a new <see cref="ConfigSectionSessionScopeSettings"/> instance.
        /// </summary>
        /// <param name="ownerType">The type, who's name will be used to prefix setting variables with</param>
        /// <param name="variableSource">The variable source to obtain settings from.</param>
        public ConfigSectionSessionScopeSettings(Type ownerType, IVariableSource variableSource)
            : base()
        {
            string sessionFactoryObjectNameSettingsKey = UniqueKey.GetTypeScopedString(ownerType, "SessionFactoryObjectName");
            string entityInterceptorObjectNameSettingsKey = UniqueKey.GetTypeScopedString(ownerType, "EntityInterceptorObjectName");
            string singleSessionSettingsKey = UniqueKey.GetTypeScopedString(ownerType, "SingleSession");
            string defaultFlushModeSettingsKey = UniqueKey.GetTypeScopedString(ownerType, "DefaultFlushMode");

            VariableAccessor variables = new VariableAccessor(variableSource);
            this.sessionFactoryObjectName = variables.GetString(sessionFactoryObjectNameSettingsKey, DEFAULT_SESSION_FACTORY_OBJECT_NAME);
            this.entityInterceptorObjectName = variables.GetString(entityInterceptorObjectNameSettingsKey, null);
            this.SingleSession = variables.GetBoolean(singleSessionSettingsKey, this.SingleSession);
            this.DefaultFlushMode = (FlushMode)variables.GetEnum(defaultFlushModeSettingsKey, this.DefaultFlushMode);

            AssertUtils.ArgumentNotNull(sessionFactoryObjectName, "sessionFactoryObjectName"); // just to be sure
        }

        /// <summary>
        /// Resolve the entityInterceptor by looking up <see cref="entityInterceptorObjectName"/> 
        /// in the root application context.
        /// </summary>
        /// <returns>The resolved <see cref="IInterceptor"/> instance or <see langword="null"/></returns>
        protected override IInterceptor ResolveEntityInterceptor()
        {
            if (StringUtils.HasText(entityInterceptorObjectName))
            {
                return (IInterceptor)ContextRegistry.GetContext().GetObject(entityInterceptorObjectName);
            }
            return null;
        }

        /// <summary>
        /// Resolve the <see cref="ISessionFactory"/> by looking up <see cref="sessionFactoryObjectName"/> 
        /// in the root application context.
        /// </summary>
        /// <returns>The resolved <see cref="ISessionFactory"/> instance or <see langword="null"/></returns>
        protected override ISessionFactory ResolveSessionFactory()
        {
            if (StringUtils.HasText(sessionFactoryObjectName))
            {
                return (ISessionFactory)ContextRegistry.GetContext().GetObject(sessionFactoryObjectName);
            }
            return null;
        }
    }
}

#region License

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

#endregion

#region Imports

using System.Collections;
using System.Web;
using Common.Logging;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class HttpApplicationConfigurer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpApplicationConfigurer));

        #region ModuleDefinitionsTable class

        private class ModuleDefinitionsTable : Hashtable
        {
            public void Add(string key, IObjectDefinition value)
            {
                lock(SyncRoot)
                {
                    base[key] = value;
                }
            }

            public IObjectDefinition this[string key]
            {
                get
                {
                    lock (SyncRoot)
                    {
                        return (IObjectDefinition) base[key];
                    }
                }
            }

            public override void Add(object key, object value)
            {
                AssertUtils.AssertArgumentType(key, "key", typeof(string), "Key must be a string");
                AssertUtils.AssertArgumentType(value, "value", typeof(IObjectDefinition), "Key must be a string");
                this.Add((string)key, (IObjectDefinition)value);
            }

            public override object this[object key]
            {
                get
                {
                    return this[(string)key];
                }
            }
        }

        #endregion

        /// <summary>
        /// Holds shared application Template
        /// </summary>
        private static volatile IObjectDefinition s_applicationDefinition = null;
        /// <summary>
        /// Holds shared modules Templates
        /// </summary>
        private static readonly ModuleDefinitionsTable s_moduleDefinitions = new ModuleDefinitionsTable();

        /// <summary>
        /// Gets or Sets the shared application template
        /// </summary>
        public IObjectDefinition ApplicationTemplate
        {
            // no need for lock because of "volatile"
            get { return s_applicationDefinition; }
            set { s_applicationDefinition = value; }
        }

        /// <summary>
        /// Gets the dictionary of shared module templates
        /// </summary>
        /// <remarks>
        /// to synchronize access to the dictionary, use <see cref="ICollection.SyncRoot"/> property.
        /// </remarks>
        public IDictionary ModuleTemplates
        {
            get { return s_moduleDefinitions; }
        }

        /// <summary>
        /// Configures the <see paramref="app"/> instance and its modules.
        /// </summary>
        /// <remarks>
        /// When called, configures <see paramref="app"/> using the <see cref="IApplicationContext"/> instance 
        /// provided in <paramref name="appContext"/> and the templates available in
        /// <see cref="ApplicationTemplate"/> and <see cref="ModuleTemplates"/>.
        /// </remarks>
        /// <param name="appContext">the application context instance to be used for resolving object references.</param>
        /// <param name="app">the <see cref="HttpApplication"/> instance to be configured.</param>
        public static void Configure(IConfigurableApplicationContext appContext, HttpApplication app)
        {
            IObjectDefinition applicationDefinition = s_applicationDefinition;
            if (s_applicationDefinition != null)
            {                
                appContext.ObjectFactory.ConfigureObject(app, "ApplicationTemplate", applicationDefinition);
            }
            
            lock(s_moduleDefinitions.SyncRoot)
            {
                HttpModuleCollection modules = app.Modules;
                foreach(DictionaryEntry moduleEntry in s_moduleDefinitions)
                {
					string moduleName = (string) moduleEntry.Key;
                    IObjectDefinition od = s_moduleDefinitions[moduleName];
                    IHttpModule module = modules[moduleName];
                    if (module != null)
                    {
                        appContext.ObjectFactory.ConfigureObject(module, moduleName, od);
                    }
                    else
                    {
                        throw ConfigurationUtils.CreateConfigurationException(string.Format("failed applying module template '{0}' - no matching module found", moduleName));
                    }
                }
            }
        }
    }
}
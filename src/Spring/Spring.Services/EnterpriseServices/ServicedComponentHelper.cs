#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

#if (!NET_1_0 && !MONO)

#region Imports

using System;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Xml;
using Spring.Context.Support;
using ConfigXmlDocument = Spring.Util.ConfigXmlDocument;

#endregion

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// This class supports <see cref="ServicedComponent"/>s exported using <see cref="EnterpriseServicesExporter"/>.
    /// and must never be used directly.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class ServicedComponentHelper
    {
        private static bool isInitialized;
        private static string componentDirectory;

        static ServicedComponentHelper()
        {
            isInitialized = false;
        }

        ///<summary>
        /// Reads in the 'xxx.spring-context.xml' configuration file associated with the specified <paramref name="component"/>.
        /// See <see cref="EnterpriseServicesExporter"/> for an in-depth description on how to export and configure COM+ components.
        ///</summary>
        private static void EnsureComponentContextRegistryInitialized(ServicedComponent component)
        {
            if (isInitialized) return;

            lock (typeof(ServicedComponentHelper))
            {
                if (isInitialized) return;
                isInitialized = true;

                // this is to ensure, that assemblies place next to the component assembly can be loaded
                // even when they are not strong named.
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                FileInfo componentAssemblyFile = new FileInfo(component.GetType().Assembly.Location);
                componentDirectory = componentAssemblyFile.Directory.FullName;
                // switch to component assembly's directory (affects resolving relative paths during context instantiation!)
                Environment.CurrentDirectory = componentDirectory;
                // read in config file
                ConfigXmlDocument configDoc = new ConfigXmlDocument();
                FileInfo configFile = new FileInfo(componentAssemblyFile.FullName + ".spring-context.xml");
                if (configFile.Exists)
                {
                    configDoc.Load(configFile.FullName);
                    XmlNode configNode = configDoc.SelectSingleNode("//context");
                    ServicedComponentContextHandler handler = new ServicedComponentContextHandler();
                    lock (ContextRegistry.SyncRoot)
                    {
                        // it might accidentially have happend, that the contextregistry has already 
                        // been initialized using the client application's app.config configuration.
                        // Most of the time this doesn't make sense to read a configuration from a 
                        // different AppDomain, thus we read in our own config file.
                        ContextRegistry.Clear();
                        handler.Create(null, null, configNode);
                    }
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Split(',')[0];
            Assembly assembly = Assembly.LoadFrom(Path.Combine(componentDirectory, name + ".dll"));
            return assembly;
        }

        ///<summary>
        /// Called by a <see cref="ServicedComponent"/> exported by <see cref="EnterpriseServicesExporter"/> 
        /// to obtain a reference to the service it proxies.
        ///</summary>
        public static object GetObject(ServicedComponent sender, string targetName)
        {
            EnsureComponentContextRegistryInitialized(sender);
            return ContextRegistry.GetContext().GetObject(targetName);
        }
    }
}

#endif
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

#region Imports

using System;
using System.Configuration;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Xml;
using Spring.Context;
using Spring.Context.Support;
using Spring.Reflection.Dynamic;
using ConfigXmlDocument = Spring.Util.ConfigXmlDocument;

#endregion

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// 
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
        ///</summary>
        ///<param name="component"></param>
        private static void EnsureComponentContextRegistryInitialized(ServicedComponent component)
        {
            if (isInitialized) return;

            lock (typeof(ServicedComponentHelper))
            {
                if (isInitialized) return;
                isInitialized = true;

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                FileInfo componentAssemblyFile = new FileInfo(component.GetType().Assembly.Location);
                componentDirectory = componentAssemblyFile.Directory.FullName;
                Environment.CurrentDirectory = componentDirectory;
                ConfigXmlDocument configDoc = new ConfigXmlDocument();
                FileInfo configFile = new FileInfo(componentAssemblyFile.FullName + ".spring-context.xml");
                if (configFile.Exists)
                {
                    configDoc.Load(configFile.FullName);
                    XmlNode configNode = configDoc.SelectSingleNode("//context");
                    ServicedComponentContextHandler handler = new ServicedComponentContextHandler();
                    lock (ContextRegistry.SyncRoot)
                    {
                        ContextRegistry.Clear();
                        handler.Create(null, null, configNode);
                    }
                }
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Split(',')[0];
            Assembly assembly = Assembly.LoadFrom(Path.Combine(componentDirectory, name + ".dll"));
            return assembly;
        }

        ///<summary>
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="targetName"></param>
        ///<returns></returns>
        public static object GetObject(ServicedComponent sender, string targetName)
        {
            EnsureComponentContextRegistryInitialized(sender);
            return ContextRegistry.GetContext().GetObject(targetName);
            //            return null;
        }

    }
}
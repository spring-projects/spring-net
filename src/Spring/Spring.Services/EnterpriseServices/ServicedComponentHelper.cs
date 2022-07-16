#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Diagnostics;
using System.EnterpriseServices;
using System.Reflection;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Util;

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
        private static IApplicationContext _appContext;

        static ServicedComponentHelper()
        {
            isInitialized = false;
        }

        ///<summary>
        /// Reads in the 'xxx.spring-context.xml' configuration file associated with the specified <paramref name="componentType"/>.
        /// See <see cref="EnterpriseServicesExporter"/> for an in-depth description on how to export and configure COM+ components.
        ///</summary>
        public static void EnsureComponentContextRegistryInitialized(Type componentType)
        {
            if (isInitialized) return;

            lock (typeof(ServicedComponentHelper))
            {
                if (isInitialized) return;

                try
                {
                    Initialize(componentType);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error configuring application context for COM component of type " + componentType + ": " + e);
                    throw;
                }

                isInitialized = true;
            }
        }

        private static void Initialize(Type componentType)
        {
            // this is to ensure, that assemblies placed next to the component assembly can be loaded
            // even when they are not strong named.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            FileInfo componentAssemblyFile = new FileInfo(componentType.Assembly.Location);
            FileInfo assemblyFile = new FileInfo(componentAssemblyFile.FullName);

            bool isRunningOutOfProcess = IsRunningOutOfProcess();
            FileInfo configFile = new FileInfo(componentAssemblyFile.FullName + ".config");

            // no config file and in-proc -> reuse app's context, error otherwise
            if (!configFile.Exists)
            {
                if (!isRunningOutOfProcess)
                {
                    // check for context with component's name
                    if (ContextRegistry.IsContextRegistered(componentType.Name))
                    {
                        Trace.WriteLine(string.Format("configuring COM InProc Server '{0}' using section <spring/context name={1}> from app.config", componentAssemblyFile.FullName, componentType.Name));
                        _appContext = ContextRegistry.GetContext(componentType.Name);
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("configuring COM InProc Server '{0}' using section <spring/context> from file '{1}'", componentAssemblyFile.FullName, configFile.FullName));
                        _appContext = ContextRegistry.GetContext();
                    }
                    return;
                }
                throw ConfigurationUtils.CreateConfigurationException("Spring-exported COM components require <spring/context> section in configuration file '" + configFile.FullName + "'");
            }

            // set and switch to component assembly's directory (affects resolving relative paths during context instantiation!)
            componentDirectory = componentAssemblyFile.Directory.FullName;
            Environment.CurrentDirectory = componentDirectory;

            if (isRunningOutOfProcess)
            {
                Trace.WriteLine(string.Format("configuring COM OutProc Server '{0}' using '{1}'", componentAssemblyFile.FullName, configFile.FullName));
                // read in config file
                ExeConfigurationSystem comConfig = new ExeConfigurationSystem(assemblyFile.FullName);
                // make the config "global" for this process, replacing any
                // existing configuration that might already have been loaded
                ConfigurationUtils.SetConfigurationSystem(comConfig, true);
                _appContext = ContextRegistry.GetContext();
            }
            else
            {
                Trace.WriteLine(string.Format("configuring COM InProc Server '{0}' using section <spring/context> from file '{1}'", componentAssemblyFile.FullName, configFile.FullName));
                _appContext = (IApplicationContext)ConfigurationReader.GetSection(new FileSystemResource(configFile.FullName), "spring/context");
            }
            if (_appContext == null)
            {
                throw ConfigurationUtils.CreateConfigurationException("Spring-exported COM components require <spring/context> section in configuration file");
            }
            Trace.WriteLine(string.Format("completed configuring COM Component '{0}' using '{1}'", componentAssemblyFile.FullName, configFile.FullName));
        }

        private static bool IsRunningOutOfProcess()
        {
            // TODO: checkout a prob. better way to find out, whether we are executing as a com server or library
            return AppDomain.CurrentDomain.SetupInformation.ApplicationName == "dllhost.exe";
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
            EnsureComponentContextRegistryInitialized(sender.GetType());
            try
            {
                return _appContext.GetObject(targetName);

            }
            catch (Exception e)
            {
                Trace.WriteLine("Error configuring application context for COM component of type " + sender.GetType() + ": " + e);
                throw;
            }
        }
    }
}

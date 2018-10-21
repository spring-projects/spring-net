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

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Reflection;
using System.Xml;

namespace Spring.Util
{
    /// <summary>
    /// Utility class for .NET configuration files management.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public static class ConfigurationUtils
    {
        private static readonly ConcurrentDictionary<string, object> cachedSections =
            new ConcurrentDictionary<string, object>();
        
        /// <summary>
        /// Parses the configuration section.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Primary purpose of this method is to allow us to parse and 
        /// load configuration sections using the same API regardless
        /// of the .NET framework version.
        /// </p>
        /// <p>
        /// If Microsoft paid a bit more attention to preserving backwards
        /// compatibility we would not even need it, but... :(
        /// </p>
        /// </remarks>
        /// <param name="sectionName">Name of the configuration section.</param>
        /// <returns>Object created by a corresponding <see cref="IConfigurationSectionHandler"/>.</returns>
        public static object GetSection(string sectionName)
        {
            return cachedSections.GetOrAdd(sectionName, DoGetSection);
        }

        private static object DoGetSection(string sectionName)
        {
            try
            {
                var name = sectionName.TrimEnd('/');
                var section = ConfigurationManager.GetSection(name);
                return section;
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw CreateConfigurationException($"Error reading section {sectionName}", ex);
            }
        }

        internal static void ClearCache()
        {
            cachedSections.Clear();
        }

        /// <summary>
        /// Refresh the configuration section.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Primary purpose of this method is to allow us to parse and 
        /// load configuration sections using the same API regardless
        /// of the .NET framework version.
        /// </p>
        /// <p>
        /// If Microsoft paid a bit more attention to preserving backwards
        /// compatibility we would not even need it, but... :(
        /// </p>
        /// </remarks>
        /// <param name="sectionName">Name of the configuration section.</param>
        public static void RefreshSection(string sectionName)
        {
            ConfigurationManager.RefreshSection(sectionName);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <param name="inner">The inner exception.</param>
        /// <param name="fileName">Name of the configuration file.</param>
        /// <param name="line">The line where exception occured.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message, Exception inner, string fileName, int line)
        {
            return new ConfigurationErrorsException(message, inner, fileName, line);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <param name="fileName">Name of the configuration file.</param>
        /// <param name="line">The line where exception occured.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message, string fileName, int line)
        {
            return CreateConfigurationException(message, null, fileName, line);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <param name="inner">The inner exception.</param>
        /// <param name="node">XML node where exception occured.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message, Exception inner, XmlNode node)
        {
            return new ConfigurationErrorsException(message, inner, node);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <param name="node">XML node where exception occured.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message, XmlNode node)
        {
            return CreateConfigurationException(message, null, node);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <param name="inner">The inner exception.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message, Exception inner)
        {
            return new ConfigurationErrorsException(message, inner);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message)
        {
            return CreateConfigurationException(message, (Exception)null);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException()
        {
            return CreateConfigurationException(null, (Exception)null);
        }

        /// <summary>
        /// Determines whether the specified exception is configuration exception.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified exception is configuration exception; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConfigurationException(Exception exception)
        {
            return exception is ConfigurationErrorsException;
        }

        /// <summary>
        /// Returns the line number of the specified node.
        /// </summary>
        /// <param name="node">Node to get the line number for.</param>
        /// <returns>The line number of the specified node.</returns>
        public static int GetLineNumber(XmlNode node)
        {
            if (node is ITextPosition)
            {
                return ((ITextPosition)node).LineNumber;
            }
            return ConfigurationErrorsException.GetLineNumber(node);
        }

        /// <summary>
        /// Returns the name of the file specified node is defined in.
        /// </summary>
        /// <param name="node">Node to get the file name for.</param>
        /// <returns>The name of the file specified node is defined in.</returns>
        public static string GetFileName(XmlNode node)
        {
            if (node is ITextPosition)
            {
                return ((ITextPosition)node).Filename;
            }
            return ConfigurationErrorsException.GetFilename(node);
        }

        /// <summary>
        /// Sets the current <see cref="System.Configuration.Internal.IInternalConfigSystem"/> to be used by <see cref="ConfigurationManager"/>.
        /// </summary>
        /// <remarks>
        /// �f <paramref name="configSystem"/> implements <see cref="IChainableConfigSystem"/>, this method invokes
        /// <see cref="IChainableConfigSystem.SetInnerConfigurationSystem"/> on the new configSystem to chain them.<br/>
        /// <b> Note, that this method requires reflection on internals of <see cref="ConfigurationManager"/></b>
        /// </remarks>
        /// <param name="configSystem">the configuration system to set</param>
        /// <param name="enforce">bypasses the check if the current system has already been initialized</param>
        /// <returns>the previous config system, if any</returns>
        public static System.Configuration.Internal.IInternalConfigSystem SetConfigurationSystem(System.Configuration.Internal.IInternalConfigSystem configSystem, bool enforce)
        {
            FieldInfo s_configSystem = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.Static | BindingFlags.NonPublic);
            // for MONO
            if (s_configSystem == null)
            {
                s_configSystem = typeof(ConfigurationManager).GetField("configSystem", BindingFlags.Static | BindingFlags.NonPublic);
            }
            System.Configuration.Internal.IInternalConfigSystem innerConfigSystem = (System.Configuration.Internal.IInternalConfigSystem)s_configSystem.GetValue(null);
            if (configSystem is IChainableConfigSystem)
            {
                ((IChainableConfigSystem)configSystem).SetInnerConfigurationSystem(innerConfigSystem);
            }

            try
            {
                MethodInfo mi = typeof(ConfigurationManager).GetMethod("SetConfigurationSystem", BindingFlags.Static | BindingFlags.NonPublic);
                if (mi == null)
                {
                    mi = typeof(ConfigurationManager).GetMethod("ChangeConfigurationSystem", BindingFlags.Static | BindingFlags.NonPublic);
                    mi.Invoke(null, new object[] { configSystem });
                }
                else
                {
                    if (enforce)
                    {
                        ResetConfigurationSystem();
                    }
                    mi.Invoke(null, new object[] { configSystem, true });
                }
            }
            catch (InvalidOperationException)
            {
                if (!enforce)
                {
                    throw;
                }
                s_configSystem.SetValue(null, configSystem);
            }

            return innerConfigSystem;
        }

        /// <summary>
        /// Resets the global configuration system instance. Use for unit testing only!
        /// </summary>
        public static void ResetConfigurationSystem()
        {
            if (SystemUtils.MonoRuntime)
            {
                return;
            }
            FieldInfo initStateRef = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
            object notStarted = Activator.CreateInstance(initStateRef.FieldType);
            initStateRef.SetValue(null, notStarted);
        }
    }
}
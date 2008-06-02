#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using System.Xml;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Utility class for .NET configuration files management.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ConfigurationUtils
    {
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
#if !NET_2_0
            return ConfigurationSettings.GetConfig(sectionName.TrimEnd('/'));
#else
            return ConfigurationManager.GetSection(sectionName.TrimEnd('/'));
#endif
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
#if !NET_2_0
            // TODO : Add support for .NET 1.x
#else
            ConfigurationManager.RefreshSection(sectionName);
#endif
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
#if !NET_2_0
            return new ConfigurationException(message, inner, fileName, line);
#else
            return new ConfigurationErrorsException(message, inner, fileName, line);
#endif
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
#if !NET_2_0
            return new ConfigurationException(message, inner, node);
#else
            return new ConfigurationErrorsException(message, inner, node);
#endif
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
#if !NET_2_0
            return new ConfigurationException(message, inner);
#else
            return new ConfigurationErrorsException(message, inner);
#endif
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <param name="message">The message to display to the client when the exception is thrown.</param>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException(string message)
        {
            return CreateConfigurationException(message, (Exception) null);
        }

        /// <summary>
        /// Creates the configuration exception.
        /// </summary>
        /// <returns>Configuration exception.</returns>
        public static Exception CreateConfigurationException()
        {
            return CreateConfigurationException(null, (Exception) null);
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
#if !NET_2_0
            return exception is ConfigurationException;
#else
            return exception is ConfigurationErrorsException;
#endif
        }

        /// <summary>
        /// Returns the line number of the specified node.
        /// </summary>
        /// <param name="node">Node to get the line number for.</param>
        /// <returns>The line number of the specified node.</returns>
        public static int GetLineNumber(XmlNode node)
        {
#if !NET_2_0
            return ConfigurationException.GetXmlNodeLineNumber(node);
#else
            return ConfigurationErrorsException.GetLineNumber(node);
#endif
        }

        /// <summary>
        /// Returns the name of the file specified node is defined in.
        /// </summary>
        /// <param name="node">Node to get the file name for.</param>
        /// <returns>The name of the file specified node is defined in.</returns>
        public static string GetFileName(XmlNode node)
        {
#if !NET_2_0
            return ConfigurationException.GetXmlNodeFilename(node);
#else
            return ConfigurationErrorsException.GetFilename(node);
#endif
        }

    }
}
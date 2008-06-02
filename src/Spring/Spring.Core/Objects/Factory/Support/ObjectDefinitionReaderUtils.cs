#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Text;
using System.Text.RegularExpressions;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Support;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Utility methods that are useful for
    /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionReader"/>
    /// implementations.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="ObjectsNamespaceParser"/>
    public sealed class ObjectDefinitionReaderUtils
    {
        /// <summary>
        /// The string used as a separator in the generation of synthetic id's
        /// for those object definitions explicitly that aren't assigned one.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If a <see cref="System.Type"/>  name or parent object definition
        /// name is not unique, "#1", "#2" etc will be appended, until such
        /// time that the name becomes unique.
        /// </p>
        /// </remarks>
        public const string GeneratedObjectIdSeparator = "#";

        /// <summary>
        /// Registers the supplied <paramref name="objectDefinition"/> with the
        /// supplied <paramref name="registry"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a convenience method that registers the
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder.ObjectDefinition"/>
        /// of the supplied <paramref name="objectDefinition"/> under the 
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder.ObjectName"/>
        /// property value of said <paramref name="objectDefinition"/>. If the
        /// supplied <paramref name="objectDefinition"/> has any
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder.Aliases"/>,
        /// then those aliases will also be registered with the supplied
        /// <paramref name="registry"/>.
        /// </p>
        /// </remarks>
        /// <param name="objectDefinition">
        /// The object definition holder containing the
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> that
        /// is to be registered.
        /// </param>
        /// <param name="registry">
        /// The registry that the supplied <paramref name="objectDefinition"/>
        /// is to be registered with.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the supplied arguments is <see langword="null"/>.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the <paramref name="objectDefinition"/> could not be registered
        /// with the <paramref name="registry"/>.
        /// </exception>
        public static void RegisterObjectDefinition(
            ObjectDefinitionHolder objectDefinition, IObjectDefinitionRegistry registry)
        {
            AssertUtils.ArgumentNotNull(objectDefinition, "objectDefinition");
            AssertUtils.ArgumentNotNull(registry, "registry");

            registry.RegisterObjectDefinition(objectDefinition.ObjectName, objectDefinition.ObjectDefinition);
            string[] aliases = objectDefinition.Aliases;
            for (int i = 0; i < aliases.Length; ++i)
            {
                string alias = aliases[i];
                registry.RegisterAlias(objectDefinition.ObjectName, alias);
            }
        }

        /// <summary>
        /// Generates an object definition name for the supplied
        /// <paramref name="objectDefinition"/> that is guaranteed to be unique
        /// within the scope of the supplied <paramref name="registry"/>.
        /// </summary>
        /// <param name="objectDefinition">
        /// The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
        /// that requires a generated name.
        /// </param>
        /// <param name="registry">
        /// The
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// that the supplied <paramref name="objectDefinition"/> is to be
        /// registered with (needed so that the uniqueness of any generated
        /// name can be guaranteed).
        /// </param>
        /// <returns>
        /// An object definition name for the supplied
        /// <paramref name="objectDefinition"/> that is guaranteed to be unique
        /// within the scope of the supplied <paramref name="registry"/> and
        /// never <cref lang="null"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the <paramref name="objectDefinition"/> or
        /// <paramref name="registry"/> arguments is <see langword="null"/>.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If a unique name cannot be generated.
        /// </exception>
        public static string GenerateObjectName(
            IConfigurableObjectDefinition objectDefinition, IObjectDefinitionRegistry registry)
        {
            AssertUtils.ArgumentNotNull(objectDefinition, "objectDefinition");
            AssertUtils.ArgumentNotNull(registry, "registry");

            string starterName = objectDefinition.ObjectTypeName;
            if (StringUtils.IsNullOrEmpty(starterName))
            {
                if (objectDefinition is ChildObjectDefinition)
                {
                    starterName = ((ChildObjectDefinition) objectDefinition).ParentName + "$child";
                }
                else if (objectDefinition.FactoryObjectName != null)
                {
                    starterName = objectDefinition.FactoryObjectName + "$created";
                }
            }
            if (StringUtils.IsNullOrEmpty(starterName))
            {
                throw new ObjectDefinitionStoreException(
                    objectDefinition.ResourceDescription, String.Empty,
                    "Unnamed object definition specifies neither 'Type' nor 'Parent' " +
                    "nor 'FactoryObject' property values so a unique name cannot be generated.");
            }
            String generatedName = starterName;
            int counter = 0;
            while (registry.ContainsObjectDefinition(generatedName))
            {
                generatedName = new StringBuilder(starterName)
                    .Append(GeneratedObjectIdSeparator).Append(++counter).ToString();
            }
            return generatedName;
        }

        /// <summary>
        /// Factory method for getting concrete
        /// <see cref="Spring.Objects.IEventHandlerValue"/> instances.
        /// </summary>
        /// <param name="methodName">
        /// The name of the event handler method. This may be straight text, a regular
        /// expression, <see langword="null"/>, or empty.
        /// </param>
        /// <param name="eventName">
        /// The name of the event being wired. This too may be straight text, a regular
        /// expression, <see langword="null"/>, or empty.
        /// </param>
        /// <returns>
        /// A concrete <see cref="Spring.Objects.IEventHandlerValue"/>
        /// instance.
        /// </returns>
        public static IEventHandlerValue CreateEventHandlerValue(
            string methodName, string eventName)
        {
            bool weAreAutowiring = false;
            if (StringUtils.HasText(eventName))
            {
                // does the value contain regular expression characters? mmm, totally trent...
                if (Regex.IsMatch(eventName, @"[\*\.\[\]\{\},\(\)\$\^\+]+"))
                {
                    // wildcarded event name
                    weAreAutowiring = true;
                }
            }
            else
            {
                // we're definitely autowiring based on the event name
                weAreAutowiring = true;
            }
            if (!weAreAutowiring)
            {
                if (StringUtils.HasText(methodName))
                {
                    // does the value contain the string ${event}?
                    if (methodName.IndexOf("${event}") >= 0)
                    {
                        // wildcarded method name
                        weAreAutowiring = true;
                    }
                }
                else
                {
                    // we're definitely autowiring based on the method name
                    weAreAutowiring = true;
                }
            }
            IEventHandlerValue myHandler;
            if (weAreAutowiring)
            {
                myHandler = new AutoWiringEventHandlerValue();
            }
            else
            {
                myHandler = new InstanceEventHandlerValue();
            }
            myHandler.EventName = eventName;
            myHandler.MethodName = methodName;
            return myHandler;
        }

        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ObjectDefinitionReaderUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private ObjectDefinitionReaderUtils()
        {
        }

        // CLOVER:ON

        #endregion
    }
}
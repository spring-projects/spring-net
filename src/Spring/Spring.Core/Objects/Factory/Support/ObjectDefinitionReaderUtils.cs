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

using System;
using System.Collections.Generic;
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
        public const string GENERATED_OBJECT_NAME_SEPARATOR = ObjectFactoryUtils.GeneratedObjectNameSeparator;

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
            IList<string> aliases = objectDefinition.Aliases;
            for (int i = 0; i < aliases.Count; ++i)
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
        /// <param name="objectDefinition">The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
        /// that requires a generated name.</param>
        /// <param name="registry">The
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// that the supplied <paramref name="objectDefinition"/> is to be
        /// registered with (needed so that the uniqueness of any generated
        /// name can be guaranteed).</param>
        /// <param name="isInnerObject">if set to <c>true</c> if the given object
        /// definition will be registed as an inner object or as a top level objener objects
        /// verses top level objects.</param>
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
            IConfigurableObjectDefinition objectDefinition, IObjectDefinitionRegistry registry, bool isInnerObject)
        {
            AssertUtils.ArgumentNotNull(objectDefinition, "objectDefinition");
            AssertUtils.ArgumentNotNull(registry, "registry");

            string generatedObjectName = objectDefinition.ObjectTypeName;
            if (StringUtils.IsNullOrEmpty(generatedObjectName))
            {
                if (objectDefinition is ChildObjectDefinition)
                {
                    generatedObjectName = ((ChildObjectDefinition)objectDefinition).ParentName + "$child";
                }
                else if (objectDefinition.FactoryObjectName != null)
                {
                    generatedObjectName = objectDefinition.FactoryObjectName + "$created";
                }
            }
            if (StringUtils.IsNullOrEmpty(generatedObjectName))
            {
                if (!isInnerObject)
                {
                    throw new ObjectDefinitionStoreException(
                        objectDefinition.ResourceDescription, String.Empty,
                        "Unnamed object definition specifies neither 'Type' nor 'Parent' " +
                        "nor 'FactoryObject' property values so a unique name cannot be generated.");
                }
                generatedObjectName = "$nested";
            }

            String id = generatedObjectName;
            if (isInnerObject)
            {
                id = generatedObjectName + GENERATED_OBJECT_NAME_SEPARATOR + ObjectUtils.GetIdentityHexString(objectDefinition);
            }
            else
            {
                int counter = -1;
                while (counter == -1 || registry.ContainsObjectDefinition(id))
                {
                    counter++;
                    id = generatedObjectName + GENERATED_OBJECT_NAME_SEPARATOR + counter;
                }
            }

            return id;
        }

        /// <summary>
        /// Generates the name of the object for a top-level object definition unique within the given object factory.
        /// </summary>
        /// <param name="definition">The object definition to generate an object name for.</param>
        /// <param name="registry">The registry to check for existing names.</param>
        /// <returns>The generated object name</returns>
        /// <exception cref="ObjectDefinitionStoreException">if no unique name can be generated for the given
        /// object definition</exception>
        public static string GenerateObjectName(IConfigurableObjectDefinition definition, IObjectDefinitionRegistry registry)
        {
            return GenerateObjectName(definition, registry, false);
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


        public static String RegisterWithGeneratedName(AbstractObjectDefinition definition, IObjectDefinitionRegistry registry)
        {
            String generatedName = GenerateObjectName(definition, registry, false);
            registry.RegisterObjectDefinition(generatedName, definition);
            return generatedName;
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
#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using Spring.Objects.Factory.Parsing;
using Spring.Collections.Generic;
using System.Reflection;

namespace Spring.Context.Attributes
{

    /// <summary>
    /// Parses classes with the <see cref="ConfigurationAttribute"/> applied to them.
    /// </summary>
    public class ConfigurationClassParser
    {
        private Collections.Generic.ISet<ConfigurationClass> _configurationClasses = new HashedSet<ConfigurationClass>();

        private Stack<ConfigurationClass> _importStack = new Stack<ConfigurationClass>();

        private IProblemReporter _problemReporter;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClassParser class.
        /// </summary>
        /// <param name="problemReporter"></param>
        public ConfigurationClassParser(IProblemReporter problemReporter)
        {
            _problemReporter = problemReporter;
        }

        /// <summary>
        /// Gets the configuration classes.
        /// </summary>
        /// <value>The configuration classes.</value>
        public Collections.Generic.ISet<ConfigurationClass> ConfigurationClasses
        {
            get { return _configurationClasses; }
        }

        /// <summary>
        /// Parses the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="objectName">Name of the object.</param>
        public void Parse(Type type, string objectName)
        {
            ProcessConfigurationClass(new ConfigurationClass(objectName, type));
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public void Validate()
        {
            foreach (ConfigurationClass configClass in ConfigurationClasses)
            {
                configClass.Validate(_problemReporter);
            }
        }

        /// <summary>
        /// Processes the configuration class.
        /// </summary>
        /// <param name="configurationClass">The configuration class.</param>
        protected void ProcessConfigurationClass(ConfigurationClass configurationClass)
        {
            DoProcessConfigurationClass(configurationClass);

            if (ConfigurationClasses.Contains(configurationClass) && configurationClass.ObjectName != null)
            {
                // Explicit object definition found, probably replacing an import.
                // Let's remove the old one and go with the new one.
                ConfigurationClasses.Remove(configurationClass);
            }
            ConfigurationClasses.Add(configurationClass);
        }

        private void DoProcessConfigurationClass(ConfigurationClass configurationClass)
        {

            Attribute[] importAttributes = Attribute.GetCustomAttributes(configurationClass.ConfigurationClassType, typeof(ImportAttribute));

            if (importAttributes.Length > 0)
            {
                foreach (Attribute importAttribute in importAttributes)
                {
                    ImportAttribute attrib = importAttribute as ImportAttribute;

                    if (null != attrib)
                    {
                        ProcessImport(configurationClass, attrib.Types);
                    }
                }
            }

            Attribute[] importResourceAttributes = Attribute.GetCustomAttributes(configurationClass.ConfigurationClassType, typeof(ImportResourceAttribute));

            if (importResourceAttributes.Length > 0)
            {
                foreach (Attribute importResourceAttribute in importResourceAttributes)
                {
                    ImportResourceAttribute attrib = importResourceAttribute as ImportResourceAttribute;

                    if (null != attrib)
                    {
                        foreach (string resource in attrib.Resources)
                        {
                            configurationClass.AddImportedResource(resource, attrib.DefinitionReader);
                        }
                    }
                }
            }

            Collections.Generic.ISet<MethodInfo> definitionMethods = GetAllMethodsWithCustomAttributeForClass(configurationClass.ConfigurationClassType, typeof(ObjectDefAttribute));
            foreach (MethodInfo definitionMethod in definitionMethods)
            {
                configurationClass.Methods.Add(new ConfigurationClassMethod(definitionMethod, configurationClass));

            }
        }

        /// <summary>
        /// Gets all methods with custom attribute for class.
        /// </summary>
        /// <param name="theClass">The class.</param>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <returns></returns>
        public static Collections.Generic.ISet<MethodInfo> GetAllMethodsWithCustomAttributeForClass(Type theClass, Type customAttribute)
        {
            Collections.Generic.ISet<MethodInfo> methods = new HashedSet<MethodInfo>();

            foreach (MethodInfo method in theClass.GetMethods())
            {
                if (Attribute.GetCustomAttribute(method, customAttribute) != null)
                {
                    methods.Add(method);
                }
            }

            return methods;
        }

        private void ProcessImport(ConfigurationClass configClass, IEnumerable<Type> classesToImport)
        {
            if (_importStack.Contains(configClass))
            {
                _problemReporter.Error(new CircularImportProblem(configClass, _importStack, configClass.ConfigurationClassType));
            }
            else
            {
                _importStack.Push(configClass);
                foreach (Type classToImport in classesToImport)
                {
                    ProcessConfigurationClass(new ConfigurationClass(null, classToImport));
                }
                _importStack.Pop();
            }
        }

        private class CircularImportProblem : Problem
        {
            public CircularImportProblem(ConfigurationClass configClass, Stack<ConfigurationClass> importStack, Type configurationClassType)
                : base(String.Format("A circular [Import] has been detected: " +
                             "Illegal attempt by [Configuration] class '{0}' to import class '{1}' as '{2}' is " +
                             "already present in the current import stack [{3}]",
                             importStack.Peek().SimpleName, configClass.SimpleName,
                             configClass.SimpleName, importStack),
                      new Location(importStack.Peek().Resource, configurationClassType)
                )
            { }

        }

    }
}

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

using System.Reflection;
using System.Text;
using Spring.Core.IO;
using Spring.Collections.Generic;
using Spring.Objects.Factory.Parsing;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Represents an instance of the metadata that has been parsed from a class with the <see cref="ConfigurationAttribute"/> applied to it.
    /// </summary>
    public class ConfigurationClass
    {
        private Type _configurationClassType;

        private readonly IDictionary<string, Type> _importedResources = new Dictionary<string, Type>();

        private readonly Collections.Generic.ISet<ConfigurationClassMethod> _methods = new HashedSet<ConfigurationClassMethod>();

        private string _objectName;

        private readonly IResource _resource;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClass class.
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="type"></param>
        public ConfigurationClass(string objectName, Type type)
        {
            _objectName = objectName;
            _configurationClassType = type;
            _resource = new ConfigurationClassAssemblyResource(type);

        }

        /// <summary>
        /// Gets the type of the configuration class.
        /// </summary>
        /// <value>The type of the configuration class.</value>
        public Type ConfigurationClassType
        {
            get
            {
                return _configurationClassType;
            }
        }

        /// <summary>
        /// Gets the imported resources.
        /// </summary>
        /// <value>The imported resources.</value>
        public IDictionary<string, Type> ImportedResources
        {
            get
            {
                return _importedResources;
            }
        }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <value>The methods.</value>
        public Collections.Generic.ISet<ConfigurationClassMethod> Methods
        {
            get
            {
                return _methods;
            }
        }

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        /// <value>The name of the object.</value>
        public string ObjectName
        {
            get
            {
                return _objectName;
            }
            set
            {
                _objectName = value;
            }
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public IResource Resource
        {
            get
            {
                return _resource;
            }
        }

        /// <summary>
        /// Gets the SimpleName of the object.
        /// </summary>
        /// <value>The simple name.</value>
        public string SimpleName
        {
            get { return ConfigurationClassType.Name; }
        }

        /// <summary>
        /// Adds the imported resource.
        /// </summary>
        /// <param name="importedResource">The imported resource.</param>
        /// <param name="readerClass">The reader class capable of interpreting the imported resource.</param>
        public void AddImportedResource(string importedResource, Type readerClass)
        {
            _importedResources.Add(importedResource, readerClass);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            return this == other || (other is ConfigurationClass && ConfigurationClassType == ((ConfigurationClass)other).ConfigurationClassType);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ConfigurationClassType.GetHashCode() * 14;
        }

        /// <summary>
        /// Validates the specified <see cref="ConfigurationClass"/> and reports all discovered violations to the provided problem reporter for appropriate action.
        /// </summary>
        /// <param name="problemReporter">The problem reporter.</param>
        public void Validate(IProblemReporter problemReporter)
        {
            // A [ObjectDef] method may only be overloaded through inheritance. No single
            // [Configuration] class may declare two [ObjectDef] methods with the same name.
            const char hashDelim = '#';
            Dictionary<String, int> methodNameCounts = new Dictionary<String, int>();
            foreach (ConfigurationClassMethod method in _methods)
            {
                String dClassName = method.MethodMetadata.DeclaringType.FullName;
                String methodName = method.MethodMetadata.Name;

                string paramTypes = ParamTypesToString(method.MethodMetadata);

                String fqMethodName = dClassName + hashDelim + methodName + paramTypes;
                if (!methodNameCounts.ContainsKey(fqMethodName))
                {
                    methodNameCounts.Add(fqMethodName, 1);
                }
                else
                {
                    int currentCount = methodNameCounts[fqMethodName];
                    methodNameCounts.Add(fqMethodName, currentCount++);
                }
            }

            foreach (String methodName in methodNameCounts.Keys)
            {
                int count = methodNameCounts[methodName];
                if (count > 1)
                {
                    String shortMethodName = methodName.Substring(methodName.IndexOf(hashDelim) + 1);
                    problemReporter.Error(new ObjectMethodOverloadingProblem(shortMethodName, count, Resource, ConfigurationClassType));
                }
            }

            if (Attribute.GetCustomAttribute(_configurationClassType, typeof(ConfigurationAttribute)) != null)
            {

                if (ConfigurationClassType.IsSealed)
                {
                    problemReporter.Error(new SealedConfigurationProblem(SimpleName, Resource, ConfigurationClassType));

                }

                foreach (ConfigurationClassMethod method in _methods)
                {
                    method.Validate(problemReporter);
                }
            }
        }

        private string ParamTypesToString(MethodInfo methodMetadata)
        {
            var result = new StringBuilder();

            foreach (var parameter in methodMetadata.GetParameters())
            {
                result.Append(parameter.ParameterType.ToString());
            }

            return result.ToString();
        }

        private class SealedConfigurationProblem : Problem
        {
            public SealedConfigurationProblem(string name, IResource resource, Type configurationClassType)
                : base(String.Format("[Configuration] class '{0}' may not be sealed. Remove the sealed modifier to continue.", name), new Location(resource, configurationClassType))
            { }

        }

        //This class is for future use when parameterized [ObjectDef] methods are supported in the future.
        //Until then, the test for only permitting zero-param [ObjectDef] methods would fail first, previnting this error from ever being reported
        private class ObjectMethodOverloadingProblem : Problem
        {
            public ObjectMethodOverloadingProblem(string methodName, int count, IResource resource, Type configurationClassType)
                : base(String.Format("[Configuration] class '{0}' has {1} overloaded [Definiton] methods named '{2}'. " +
                    "Only one [ObjectDef] method of a given name is allowed within each [Configuration] class.", configurationClassType.Name, count, methodName), new Location(resource, configurationClassType))
            { }

        }

    }
}

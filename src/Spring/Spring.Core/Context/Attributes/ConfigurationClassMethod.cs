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
using Spring.Objects.Factory.Parsing;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Represents a <see cref="ConfigurationAttribute"/> class method marked with the <see cref="ObjectDefAttribute"/>.
    /// </summary>
    public class ConfigurationClassMethod
    {
        private readonly ConfigurationClass _configurationClass;

        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the ConfigurationClassMethod class.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="configurationClass"></param>
        public ConfigurationClassMethod(MethodInfo methodInfo, ConfigurationClass configurationClass)
        {
            _methodInfo = methodInfo;
            _configurationClass = configurationClass;
        }

        /// <summary>
        /// Gets the configuration class.
        /// </summary>
        /// <value>The configuration class.</value>
        public ConfigurationClass ConfigurationClass
        {
            get { return _configurationClass; }
        }

        /// <summary>
        /// Gets the method metadata.
        /// </summary>
        /// <value>The method metadata.</value>
        public MethodInfo MethodMetadata
        {
            get { return _methodInfo; }
        }

        /// <summary>
        /// Gets the resource location.
        /// </summary>
        /// <value>The resource location.</value>
        public Location ResourceLocation
        {
            get { return new Location(_configurationClass.Resource, _methodInfo); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}:name={1},declaringClass={2}", GetType().Name, _methodInfo.Name,
                                 _methodInfo.DeclaringType.FullName);
        }

        /// <summary>
        /// Validates the specified problem reporter.
        /// </summary>
        /// <param name="problemReporter">The problem reporter.</param>
        public void Validate(IProblemReporter problemReporter)
        {
            //TODO: investigate whether this should be "if method has ObjectDef attribute" instead of "if class has Configuration attribute"
            if (
                Attribute.GetCustomAttribute(ConfigurationClass.ConfigurationClassType, typeof (ConfigurationAttribute)) !=
                null)
            {

                if (MethodMetadata.IsStatic)
                {
                    problemReporter.Error(new StaticMethodError(MethodMetadata.Name, ResourceLocation));
                }

                if (!MethodMetadata.IsVirtual)
                {
                    problemReporter.Error(new NonVirtualMethodError(MethodMetadata.Name, ResourceLocation));
                }

                if (MethodMetadata.GetParameters().Length != 0)
                {
                    problemReporter.Error(new MethodWithParametersError(MethodMetadata.Name, ResourceLocation));
                }
            }
        }

        private class MethodWithParametersError : Problem
        {
            public MethodWithParametersError(string methodName, Location location)
                : base(
                    String.Format(
                        "Method '{0}' must not accept parameters; remove the method's parameters to continue.",
                        methodName), location)
            {
            }
        }

        private class NonVirtualMethodError : Problem
        {
            public NonVirtualMethodError(string methodName, Location location)
                : base(String.Format("Method '{0}' must be public virtual; change the method's modifiers to continue.",
                                     methodName), location)
            {
            }
        }

        private class StaticMethodError : Problem
        {
            public StaticMethodError(string methodName, Location location)
                : base(
                    String.Format("Method '{0}' must not be static; remove the method's static modifier to continue.",
                                  methodName), location)
            {
            }
        }
    }
}

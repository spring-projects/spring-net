#region License

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

#endregion

using System.Reflection;
using Spring.Util;

namespace Spring.Core
{
    /// <summary>
    /// Helper class that encapsulates the specification of a method parameter, i.e.
    /// a MethodInfo or ConstructorInfo plus a parameter index.
    /// Useful as a specification object to pass along.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rob Harrop</author>
    /// <author>Mark Pollack (.NET)</author>
    public class MethodParameter
    {
        private MethodInfo methodInfo;

        private ConstructorInfo constructorInfo;

        private readonly int parameterIndex;

        private Type parameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodParameter"/> class for the given
        /// MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to specify a parameter for.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        public MethodParameter(MethodInfo methodInfo, int parameterIndex)
        {
            this.methodInfo = methodInfo;
            this.parameterIndex = parameterIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodParameter"/> class.
        /// </summary>
        /// <param name="constructorInfo">The ConstructorInfo to specify a parameter for.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        public MethodParameter(ConstructorInfo constructorInfo, int parameterIndex)
        {
            this.constructorInfo = constructorInfo;
            this.parameterIndex = parameterIndex;
        }

        /// <summary>
        /// Gets the type of the method/constructor parameter.
        /// </summary>
        /// <value>The type of the parameter. (never <code>null</code>)</value>
        public Type ParameterType
        {
            get
            {
                if (this.parameterType == null)
                {
                    this.parameterType = (this.methodInfo != null
                                              ? ReflectionUtils.GetParameterTypes(this.methodInfo.GetParameters())[parameterIndex]
                                              : ReflectionUtils.GetParameterTypes(this.constructorInfo.GetParameters())[parameterIndex]);
                }
                return this.parameterType;
            }
        }

        /// <summary>
        /// Create a new MethodParameter for the given method or donstructor.
        /// This is a convenience constructor for scenarios where a
        /// Method or Constructor reference is treated in a generic fashion.
        /// </summary>
        /// <param name="methodOrConstructorInfo">The method or constructor to specify a parameter for.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <returns>the corresponding MethodParameter instance</returns>
        public static MethodParameter ForMethodOrConstructor(object methodOrConstructorInfo, int parameterIndex)
        {
            if (methodOrConstructorInfo is MethodInfo)
            {
                return new MethodParameter((MethodInfo) methodOrConstructorInfo, parameterIndex);
            } else if (methodOrConstructorInfo is ConstructorInfo)
            {
                return new MethodParameter((ConstructorInfo) methodOrConstructorInfo, parameterIndex);
            } else
            {
                throw new ArgumentException("Given object [" + methodOrConstructorInfo + "] is nieth a MethodInfo nor a ConstructorInfo");
            }
        }

        /// <summary>
        /// Parameters the name of the method/constructor parameter.
        /// </summary>
        /// <returns>the parameter name.</returns>
        public string ParameterName()
        {
            if (methodInfo != null)
            {
                return methodInfo.GetParameters()[parameterIndex].Name;
            } else
            {
                return constructorInfo.GetParameters()[parameterIndex].Name;
            }
        }

        /// <summary>
        /// Gets the wrapped MethodInfo, if any.  Note Either MethodInfo or ConstructorInfo is available.
        /// </summary>
        /// <value>The MethodInfo, or <code>null</code> if none.</value>
        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        /// <summary>
        /// Gets wrapped ConstructorInfo, if any.  Note Either MethodInfo or ConstructorInfo is available.
        /// </summary>
        /// <value>The ConstructorInfo, or <code>null</code> if none</value>
        public ConstructorInfo ConstructorInfo
        {
            get { return constructorInfo; }
        }

        /// <summary>
        /// Return the annotations associated with the specific method/constructor parameter.
        /// </summary>
        public Attribute[] ParameterAttributes
        {
            get
            {
                if (methodInfo != null)
                    return Attribute.GetCustomAttributes(methodInfo.GetParameters()[parameterIndex]);
                else
                    return Attribute.GetCustomAttributes(constructorInfo.GetParameters()[parameterIndex]);
            }
        }

        /// <summary>
        /// Return the annotations associated with the target method/constructor itself.
        /// </summary>
        public Attribute[] MethodAttributes
        {
            get
            {
                if (methodInfo != null)
                    return Attribute.GetCustomAttributes(methodInfo);
                else
                    return Attribute.GetCustomAttributes(constructorInfo);
            }
        }

    }
}

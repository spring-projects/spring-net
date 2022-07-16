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
using System.Runtime.Serialization;

using Spring.Util;

namespace Spring.Aop.Support
{
    /// <summary>
    /// MethodMatcher that looks for a specific attribute being present on the
    /// method (checking both the method on the onviked interface, if any and the corresponding
    /// method on the target class
    /// </summary>
    /// <author>Juergen hoeller</author>
    /// <author>Mark Pollack</author>
    /// <seealso cref="AttributeMatchingPointcut"/>
    [Serializable]
    public class AttributeMethodMatcher : StaticMethodMatcher, ISerializable
    {
        private readonly Type attributeType;


        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMethodMatcher"/> class for the
        /// given atribute type.
        /// </summary>
        /// <param name="attributeType">Type of the attribute to look for.</param>
        public AttributeMethodMatcher(Type attributeType)
        {
            ValidateAttributeTypeArgument(attributeType);
            this.attributeType = attributeType;
        }

        /// <inheritdoc />
        private AttributeMethodMatcher(SerializationInfo info, StreamingContext context)
        {
            var type = info.GetString("AttributeType");
            attributeType = type != null ? Type.GetType(type) : null;
        }

        /// <inheritdoc />
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AttributeType", attributeType?.AssemblyQualifiedName);
        }

        /// <summary>
        /// Does the supplied <paramref name="method"/> satisfy this matcher?
        /// </summary>
        /// <param name="method">The candidate method.</param>
        /// <param name="targetType">The target <see cref="System.Type"/> (may be <see langword="null"/>,
        /// in which case the candidate <see cref="System.Type"/> must be taken
        /// to be the <paramref name="method"/>'s declaring class).</param>
        /// <returns>
        /// 	<see langword="true"/> if this this method matches statically.
        /// </returns>
        /// <remarks>
        /// 	<p>
        /// Must be implemented by a derived class in order to specify matching
        /// rules.
        /// </p>
        /// </remarks>
        public override bool Matches(MethodInfo method, Type targetType)
        {
            if (method.IsDefined(attributeType, true))
            {
                // Checks whether the attribute is defined on the method or a super definition of the method
                // but does not check attributes on implemented interfaces.
                return true;
            }
            else
            {

                    Type[] parameterTypes = ReflectionUtils.GetParameterTypes(method);

                    // Also check whether the attribute is defined on a method implemented from an interface.
                    // First find all interfaces for the type that contains the method.
                    // Next, check each interface for the presence of the attribute on the corresponding
                    // method from the interface.
                    foreach (Type interfaceType in method.DeclaringType.GetInterfaces())
                    {
                        MethodInfo intfMethod = interfaceType.GetMethod(method.Name, parameterTypes);
                        if (intfMethod != null && intfMethod.IsDefined(attributeType, true))
                        {
                            return true;
                        }
                    }

                return false;
            }
        }

        private static void ValidateAttributeTypeArgument(Type attributeType)
        {
            AssertUtils.ArgumentNotNull(attributeType, "attributeType");
            if (!typeof(Attribute).IsAssignableFrom(attributeType))
            {
                throw new ArgumentException(
                    string.Format(
                        "The [{0}] Type must be derived from the [System.Attribute] class.",
                        attributeType));
            }
        }
    }
}

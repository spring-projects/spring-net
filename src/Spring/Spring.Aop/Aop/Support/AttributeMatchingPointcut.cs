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

using Spring.Util;

namespace Spring.Aop.Support
{
    /// <summary>
    /// Pointcut that looks for a specific attribute being present on a class or
    /// method.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class AttributeMatchingPointcut : IPointcut
    {
        private readonly ITypeFilter typeFilter;
        private readonly IMethodMatcher methodMatcher;


        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatchingPointcut"/> class for the
        /// given attribute type.
        /// </summary>
        /// <param name="attributeType">Type of the attribute to look for at the class level.</param>
        public AttributeMatchingPointcut(Type attributeType)
        {
            ValidateAttributeTypeArgument(attributeType);
            this.typeFilter = new AttributeTypeFilter(attributeType);
            this.methodMatcher = TrueMethodMatcher.True;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatchingPointcut"/> class for the
        /// given attribute type
        /// </summary>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="checkInherited">if set to <c>true</c> [check inherited].</param>
        public AttributeMatchingPointcut(Type attributeType, bool checkInherited)
        {
            ValidateAttributeTypeArgument(attributeType);
            this.typeFilter = new AttributeTypeFilter(attributeType, checkInherited);
            this.methodMatcher = TrueMethodMatcher.True;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatchingPointcut"/> class for the given
        /// attribute type
        /// </summary>
        /// <param name="classAttributeType">The attribute type to look for at the class level.</param>
        /// <param name="methodAttributeType">The attribute type to look for at the method attribute.</param>
        public AttributeMatchingPointcut(Type classAttributeType, Type methodAttributeType)
        {
            AssertUtils.IsTrue(classAttributeType != null || methodAttributeType != null,
                               "Either Type attribute type or Method attribute type needs to be specified (or both)");

            if (classAttributeType != null)
            {
                this.typeFilter = new AttributeTypeFilter(classAttributeType);
            } else
            {
                this.typeFilter = TrueTypeFilter.True;
            }

            if (methodAttributeType != null)
            {
                this.methodMatcher = new AttributeMethodMatcher(methodAttributeType);
            } else
            {
                this.methodMatcher = TrueMethodMatcher.True;
            }
        }

        /// <summary>
        /// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
        /// </summary>
        /// <value>The current <see cref="Spring.Aop.ITypeFilter"/>.</value>
        public ITypeFilter TypeFilter
        {
            get { return this.typeFilter; }
        }

        /// <summary>
        /// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
        /// </summary>
        /// <value>The current <see cref="Spring.Aop.IMethodMatcher"/>.</value>
        public IMethodMatcher MethodMatcher
        {
            get { return this.methodMatcher; }
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

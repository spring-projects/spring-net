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
    /// ITypeFilter that looks for a specific attribute being present on a class
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack  (.NET)</author>
    public class AttributeTypeFilter : ITypeFilter
    {
        private readonly Type attributeType;
        private readonly bool checkInherited;

        /// <summary>
        /// The attribute <see cref="Type"/> for this filter.
        /// </summary>
        public Type AttributeType
        {
            get { return attributeType; }
        }

        /// <summary>
        /// Indicates, whether this filter considers base types for filtering.
        /// </summary>
        public bool CheckInherited
        {
            get { return checkInherited; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeTypeFilter"/> class for the
        /// given attribute type.
        /// </summary>
        /// <param name="attributeType">Type of the attribute to look for.</param>
        public AttributeTypeFilter(Type attributeType) : this(attributeType, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeTypeFilter"/> class for the
        /// given attribute type.
        /// </summary>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="checkInherited">if set to <c>true</c> [check inherited].</param>
        public AttributeTypeFilter(Type attributeType, bool checkInherited)
        {
            #region parameter validation
            AssertUtils.ArgumentNotNull(attributeType, "attributeType");
            if (!typeof(Attribute).IsAssignableFrom(attributeType))
            {
                throw new ArgumentException(
                    string.Format(
                        "The [{0}] Type must be derived from the [System.Attribute] class.",
                        attributeType));
            }
            #endregion
            this.attributeType = attributeType;
            this.checkInherited = checkInherited;
        }


        /// <summary>
        /// Should the pointcut apply to the supplied <see cref="System.Type"/>?
        /// </summary>
        /// <param name="type">The candidate <see cref="System.Type"/>.</param>
        /// <returns>
        /// 	<see langword="true"/> if the advice should apply to the supplied
        /// <paramref name="type"/>
        /// </returns>
        public bool Matches(Type type)
        {
            if (checkInherited)
            {
                return AttributeUtils.FindAttribute(type, attributeType) != null;
            }
            else
            {
                object[] atts = type.GetCustomAttributes(attributeType, false);
                return ArrayUtils.HasLength(atts);
            }
        }


    }
}

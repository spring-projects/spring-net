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

namespace Spring.Context.Attributes.TypeFilters
{

    /// <summary>
    /// A simple filter which matches classes with a given attribute,
    /// checking inherited annotations as well.
    /// </summary>
    public class AttributeTypeFilter : AbstractLoadTypeFilter
    {

        /// <summary>
        /// Creates a Type Filter with required type attribute
        /// </summary>
        /// <param name="expression"></param>
        public AttributeTypeFilter(string expression)
        {
            GetRequiredType(expression);
        }

        /// <summary>
        /// Determine a match based on the given type object.
        /// </summary>
        /// <param name="type">Type to compare against</param>
        /// <returns>true if there is a match; false is there is no match</returns>
        public override bool Match(Type type)
        {
            if (RequiredType == null)
                return false;

            return (Attribute.GetCustomAttribute(type, RequiredType) != null);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("Required Type: {0}", RequiredType != null ? RequiredType.FullName : "");
        }

    }
}

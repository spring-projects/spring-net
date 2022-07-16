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

using Spring.Aop.Support;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// AutoProxyCreator, that identifies objects to proxy by matching their <see cref="Type.FullName"/> against a list of patterns.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class TypeNameAutoProxyCreator : AbstractFilteringAutoProxyCreator
    {
        private TypeNameTypeFilter _typeNameFilter = null;

        ///<summary>
        /// The list of patterns to match <see cref="Type.FullName"/> against. For pattern syntax, see <see cref="TypeNameTypeFilter"/>
        ///</summary>
        public string[] TypeNames
        {
            get
            {
                if (_typeNameFilter != null)
                {
                    return _typeNameFilter.TypeNamePatterns;
                }
                return null;
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "TypeNames");
                _typeNameFilter = new TypeNameTypeFilter(value);
            }
        }

        /// <summary>
        /// Decide, whether the given object is eligible for proxying.
        /// </summary>
        /// <remarks>
        /// Override this method to allow or reject proxying for the given object.
        /// </remarks>
        /// <param name="targetType">the object's type</param>
        /// <param name="targetName">the name of the object</param>
        /// <seealso cref="AbstractAutoProxyCreator.ShouldSkip"/>
        /// <returns>whether the given object shall be proxied.</returns>
        protected override bool IsEligibleForProxying(Type targetType, string targetName)
        {
            AssertUtils.ArgumentNotNull(_typeNameFilter, "TypeNames");

            bool shallProxy = _typeNameFilter.Matches(targetType);
            return shallProxy;
        }
    }
}

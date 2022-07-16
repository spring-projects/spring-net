#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Spring.Util;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// An AutoProxyCreator, that identifies objects to be proxied by checking <see cref="Attribute"/>s defined on their type.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class AttributeAutoProxyCreator : AbstractFilteringAutoProxyCreator
    {
        private bool _checkInherited = false;
        private Type[] _attributeTypes = null;

        /// <summary>
        /// Indicates, whether to consider base types for filtering when checking declared attributes. Defaults to <c>false</c>.
        /// </summary>
        public bool CheckInherited
        {
            get { return _checkInherited; }
            set { _checkInherited = value; }
        }

        /// <summary>
        /// The list of attribute types marking object types as eligible for auto-proxying by this AutoProxyCreator. Must not be <c>null</c>.
        /// </summary>
        public Type[] AttributeTypes
        {
            get { return _attributeTypes; }
            set
            {
                AssertUtils.ArgumentNotNull( value, "AttributeTypes" );
                _attributeTypes = value;
            }
        }

        /// <summary>
        /// Determines, whether the given object shall be proxied by matching <paramref name="targetType"/> against <see cref="AttributeTypes"/>.
        /// </summary>
        /// <param name="targetType">the object's type</param>
        /// <param name="targetName">the name of the object</param>
        protected override bool IsEligibleForProxying( Type targetType, string targetName )
        {
            AssertUtils.ArgumentNotNull(this.AttributeTypes, "AttributeTypes");

            bool shallProxy = IsAnnotatedWithAnyOfAttribute( targetType, this.AttributeTypes, this.CheckInherited );
            return shallProxy;
        }

        /// <summary>
        /// Checks if <paramref name="objectType"/> is annotated with any of the attributes within the given list of <paramref name="attributeTypes"/>.
        /// </summary>
        /// <param name="objectType">the object's type</param>
        /// <param name="attributeTypes">the list of <see cref="Attribute"/> types to match agains.</param>
        /// <param name="checkInherited">whether to check base classes and intefaces for any of the given attributes.</param>
        /// <returns><see langword="true"/> if any of the attributes is found</returns>
        protected virtual bool IsAnnotatedWithAnyOfAttribute( Type objectType, Type[] attributeTypes, bool checkInherited )
        {
            foreach(Type attributeType in attributeTypes)
            {
                if (IsAnnotatedWithAttribute(objectType, attributeType, checkInherited))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if <paramref name="objectType"/> is annotated with the specified <paramref name="attributeType"/>.
        /// </summary>
        /// <param name="objectType">the object's type</param>
        /// <param name="attributeType">the <see cref="Attribute"/> type to match agains.</param>
        /// <param name="checkInherited">whether to check base classes and intefaces for the specified attribute.</param>
        /// <returns><see langword="true"/> if the attributes is found</returns>
        protected virtual bool IsAnnotatedWithAttribute( Type objectType, Type attributeType, bool checkInherited )
        {
            if (checkInherited)
            {
                return AttributeUtils.FindAttribute( objectType, attributeType ) != null;
            }
            else
            {
                object[] atts = objectType.GetCustomAttributes( attributeType, false );
                return ArrayUtils.HasLength( atts );
            }
        }
    }
}

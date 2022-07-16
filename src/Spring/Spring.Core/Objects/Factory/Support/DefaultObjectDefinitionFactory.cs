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

using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Default implementation of the
	/// <see cref="IObjectDefinitionFactory"/>
	/// interface.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Does <b>not</b> support per <see cref="System.AppDomain"/>
	/// <see cref="System.Type"/> loading.
	/// </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
    [Serializable]
    public class DefaultObjectDefinitionFactory : IObjectDefinitionFactory
	{
	    /// <summary>
        /// Factory style method for getting concrete
        /// <see cref="IConfigurableObjectDefinition"/>
        /// instances.
        /// </summary>
        /// /// <remarks>If no parent is specified, a RootObjectDefinition is created, otherwise a
        /// ChildObjectDefinition.</remarks>
        /// <param name="typeName">The <see cref="System.Type"/> of the defined object.</param>
        /// <param name="parent">The name of the parent object definition (if any).</param>
        /// <param name="domain">The <see cref="System.AppDomain"/> against which any class names
        /// will be resolved into <see cref="System.Type"/> instances.</param>
        /// <returns>
        /// An
        /// <see cref="IConfigurableObjectDefinition"/>
        /// instance.
        /// </returns>
	    public virtual AbstractObjectDefinition CreateObjectDefinition(string typeName, string parent, AppDomain domain)
	    {
            Type objectType = null;
            if (StringUtils.HasText(typeName) && domain != null)
            {
                try
                {
                    objectType = TypeResolutionUtils.ResolveType(typeName);
                }
                // try later....
                catch { }
            }
            if (StringUtils.IsNullOrEmpty(parent))
            {
                if (objectType != null)
                {
                    return new RootObjectDefinition(objectType);

                }
                else
                {
                    RootObjectDefinition rootObjectDefinition = new RootObjectDefinition();
                    rootObjectDefinition.ObjectTypeName = typeName;
                    return rootObjectDefinition;
                }
            }
            else
            {
                if (objectType != null)
                {
                    ChildObjectDefinition childObjectDefinition = new ChildObjectDefinition(parent);
                    childObjectDefinition.ObjectType = objectType;
                    return childObjectDefinition;
                }
                else
                {
                    ChildObjectDefinition childObjectDefinition = new ChildObjectDefinition(parent);
                    childObjectDefinition.ObjectTypeName = typeName;
                    return childObjectDefinition;
                }
            }
	    }
	}
}

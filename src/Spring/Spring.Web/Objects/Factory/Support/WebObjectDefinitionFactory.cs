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

#region Imports

using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Custom implementation of <see cref="IObjectDefinitionFactory"/>
    /// for web applications.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This implementation adds support for .aspx pages and scoped objects.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class WebObjectDefinitionFactory : DefaultObjectDefinitionFactory
    {

        /// <summary>
        /// Factory style method for getting concrete
        /// <see cref="IConfigurableObjectDefinition"/>
        /// instances.
        /// </summary>
        /// <remarks>If no parent is specified, a RootWebObjectDefinition is created, otherwise a 
        /// ChildWebObjectDefinition.</remarks>
        /// <param name="typeName">The <see cref="System.Type"/> of the defined object.</param>
        /// <param name="parent">The name of the parent object definition (if any).</param>
        /// <param name="domain">The <see cref="System.AppDomain"/> against which any class names
        /// will be resolved into <see cref="System.Type"/> instances.</param>
        /// <returns>
        /// An
        /// <see cref="IConfigurableObjectDefinition"/>
        /// instance.
        /// </returns>
        public override AbstractObjectDefinition CreateObjectDefinition(string typeName, string parent, AppDomain domain)
        {
            Type objectType = null;
            bool isPage = StringUtils.HasText(typeName) && typeName.ToLower().EndsWith(".aspx");
            bool isControl = StringUtils.HasText(typeName) && 
                             (typeName.ToLower().EndsWith(".ascx") || typeName.ToLower().EndsWith(".master"));
            
            if (!(isPage || isControl) && StringUtils.HasText(typeName) && domain != null)
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
                    return new RootWebObjectDefinition(objectType, new ConstructorArgumentValues(), new MutablePropertyValues());
                }
                else if (isPage)
                {
                    return new RootWebObjectDefinition(typeName, new MutablePropertyValues());
                }
                else
                {
                    return new RootWebObjectDefinition(typeName, new ConstructorArgumentValues(), new MutablePropertyValues());
                }
            }
            else
            {
                if (objectType != null)
                {
                    return new ChildWebObjectDefinition(parent, objectType, new ConstructorArgumentValues(), new MutablePropertyValues());
                }
                else if (isPage)
                {
                    return new ChildWebObjectDefinition(parent, typeName, new MutablePropertyValues());
                }
                else
                {
                    return new ChildWebObjectDefinition(parent, typeName, new ConstructorArgumentValues(), new MutablePropertyValues());
                }
            }        
        }

    }
}

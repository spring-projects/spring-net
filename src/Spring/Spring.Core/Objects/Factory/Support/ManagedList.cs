#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Globalization;

using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Tag subclass used to hold a list of managed elements.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: ManagedList.cs,v 1.15 2007/11/26 14:15:54 bbaia Exp $</version>
    [Serializable]
    public class ManagedList : ArrayList, IManagedCollection
    {
        private string elementTypeName;

        /// <summary>
        /// Gets or sets the unresolved name for the <see cref="System.Type"/> 
        /// of the elements of this managed list.
        /// </summary>
        /// <value>The unresolved name for the type of the elements of this managed list.</value>
        public string ElementTypeName
        {
            get { return this.elementTypeName; }
            set { this.elementTypeName = value; }
        }

        /// <summary>
        /// Resolves this managed collection at runtime.
        /// </summary>
        /// <param name="objectName">
        /// The name of the top level object that is having the value of one of it's
        /// collection properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named top level object.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="resolver">
        /// The callback that will actually do the donkey work of resolving
        /// this managed collection.
        /// </param>
        /// <returns>A fully resolved collection.</returns>
        public ICollection Resolve(string objectName, RootObjectDefinition definition, string propertyName, ManagedCollectionElementResolver resolver)
        {
            IList list;

            Type elementType = null;
            if (StringUtils.HasText(this.elementTypeName))
            {
                elementType = TypeResolutionUtils.ResolveType(this.elementTypeName);
            }
#if NET_2_0
            if (elementType == null)
            {
                list = new ArrayList();
            }
            else
            {
                // CLOVER:ON
                Type type = typeof(List<>);
                Type[] genericArgs = new Type[1] { elementType };
                type = type.MakeGenericType(genericArgs);

                list = (IList)ObjectUtils.InstantiateType(type);
                // CLOVER:OFF
            }
#else
            list = new ArrayList();
#endif
            for (int i = 0; i < Count; ++i)
            {
                object element = this[i];
                object resolvedElement =
                        resolver(objectName, definition, String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", propertyName, i), element);

                if (elementType != null)
                {
                    try
                    {
                        resolvedElement = TypeConversionUtils.ConvertValueIfNecessary(elementType, resolvedElement, propertyName + "[" + i + "]");
                    }
                    catch (TypeMismatchException)
                    {
                        throw new TypeMismatchException(
                            String.Format(
                                    "Unable to convert managed list element '{0}' from [{1}] into [{2}] during initialization"
                                    + " of property '{3}' for object '{4}'. Do you have an appropriate type converter registered?", 
                                    resolvedElement, resolvedElement.GetType(), elementType, propertyName, objectName));
                    }
                }

                list.Add(resolvedElement);
            }

            return list;
        }
    }
}
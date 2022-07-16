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

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Object instantiation strategy for use in
    /// <see cref="Spring.Objects.Factory.Support.WebObjectFactory"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This strategy checks if objects id ASP.Net page and if it is uses
    /// PageParser to compile and instantiate page instance. Otherwise it
    /// delagates call to its parent.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class WebInstantiationStrategy : MethodInjectingInstantiationStrategy
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.WebInstantiationStrategy"/> class.
        /// </summary>
        public WebInstantiationStrategy()
        {}

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </summary>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="name">
        /// The name associated with the object definition. The name can be the null
        /// or zero length string if we're autowiring an object that doesn't belong
        /// to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        public override object Instantiate(
            RootObjectDefinition definition, string name, IObjectFactory factory)
        {
            if (definition is IWebObjectDefinition && ((IWebObjectDefinition) definition).IsPage)
            {
                return WebObjectUtils.CreatePageInstance(((IWebObjectDefinition) definition).PageName);
            }
            else
            {
                return base.Instantiate(definition, name, factory);
            }
        }

    }
}

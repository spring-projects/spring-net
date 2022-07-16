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

using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Holder for an <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> with
    /// name and aliases.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Recognized by
    /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory"/>
    /// for inner object definitions. Registered by
    /// <see cref="ObjectsNamespaceParser"/>,
    /// which also uses it as general holder for a parsed object definition.
    /// </p>
    /// <p>
    /// Can also be used for programmatic registration of inner object
    /// definitions. If you don't care about the functionality offered by the
    /// <see cref="Spring.Objects.Factory.IObjectNameAware"/> interface and the like,
    /// registering <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
    /// or <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/> is good enough.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Simon White (.NET)</author>
    [Serializable]
    public class ObjectDefinitionHolder
    {
        private readonly IObjectDefinition objectDefinition;
        private readonly string objectName;
        private readonly List<string> aliases;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ObjectDefinitionHolder"/> class.
        /// </summary>
        /// <param name="definition">
        /// The object definition to be held by this instance.
        /// </param>
        /// <param name="name">
        /// The name of the object definition.
        /// </param>
        public ObjectDefinitionHolder(IObjectDefinition definition, string name)
            : this(definition, name, StringUtils.EmptyStrings)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ObjectDefinitionHolder"/> class.
        /// </summary>
        /// <param name="definition">
        /// The object definition to be held by this instance.
        /// </param>
        /// <param name="name">The name of the object.</param>
        /// <param name="aliases">
        /// Any aliases for the supplied <paramref name="definition"/>
        /// </param>
        public ObjectDefinitionHolder(
            IObjectDefinition definition,
            string name,
            IReadOnlyList<string> aliases)
        {
            objectDefinition = definition;
            objectName = name;
            this.aliases = aliases != null && aliases.Count > 0
                ? new List<string>(aliases)
                : null;
        }

        /// <summary>
        /// The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> held by this
        /// instance.
        /// </summary>
        public IObjectDefinition ObjectDefinition => objectDefinition;

        /// <summary>
        /// The name of the object definition.
        /// </summary>
        public string ObjectName => objectName;

        /// <summary>
        /// Any aliases for the object definition.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Guaranteed to never return <cref lang="null"/>; if the associated
        /// <see cref="ObjectDefinition"/>
        /// does not have any aliases associated with it, then an empty
        /// <see cref="System.String"/> array will be returned.
        /// </p>
        /// </remarks>
        public IReadOnlyList<string> Aliases => aliases ?? StringUtils.EmptyStringsList;

        /// <summary>
        /// Checks wether a givin candidate name has a defined object or alias
        /// </summary>
        /// <param name="candidateName">name to check if exists</param>
        /// <returns></returns>
        public bool MatchesName(string candidateName)
        {
            return !string.IsNullOrEmpty(candidateName)
                   && (candidateName == ObjectName || aliases?.Contains(candidateName) == true);
        }
    }
}

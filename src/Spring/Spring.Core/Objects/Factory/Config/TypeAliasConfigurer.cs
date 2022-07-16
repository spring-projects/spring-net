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

using System.Collections;
using Spring.Core.TypeResolution;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// implementation that allows for convenient registration of custom
    /// type aliases.
    /// </summary>
    /// <remarks>
    /// Type aliases can be used instead of fully qualified type names anywhere
    /// a type name is expected in a Spring.NET configuration file.
    /// <p>
    /// Because the <see cref="Spring.Objects.Factory.Config.TypeAliasConfigurer"/>
    /// class implements the
    /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// interface, instances of this class that have been exposed in the
    /// scope of an
    /// <see cref="Spring.Context.IApplicationContext"/> will
    /// <i>automatically</i> be picked up by the application context and made
    /// available to the IoC container whenever resolution of type aliases is required.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// <seealso cref="Spring.Context.IApplicationContext"/>
    [Serializable]
    public class TypeAliasConfigurer : AbstractConfigurer
    {
        private IDictionary types;


        /// <summary>
        /// The type aliases to register.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The <see cref="System.Collections.IDictionary"/> has the
        /// contains the alias name as the key and type as the value.
        /// The key name can either be a string or an object, in which case
        /// ToString() will be used to obtain the string name.
        /// the value can be the fully qualified name of the type as a string or
        /// an actual <see cref="System.Type"/> of the class that
        /// being aliased.
        /// </p>
        /// </remarks>
        public IDictionary TypeAliases
        {
            set { types = value; }
        }

        /// <summary>
        /// Registers any type aliases.  The supplied
        /// <paramref name="factory"/> is not used since type aliases
        /// are registered with a global <see cref="TypeRegistry"/>
        /// </summary>
        /// <param name="factory">
        /// The object factory.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public override void PostProcessObjectFactory(
            IConfigurableListableObjectFactory factory)
        {
            if (types != null)
            {
                foreach (DictionaryEntry entry in types)
                {
                    string alias = entry.Key.ToString();
                    Type type = ResolveRequiredType(entry.Value, "value", "custom type alias");
                    TypeRegistry.RegisterType(alias, type);
                }
            }
        }


    }
}

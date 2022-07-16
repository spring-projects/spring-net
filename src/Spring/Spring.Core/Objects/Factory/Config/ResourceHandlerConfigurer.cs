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
using Spring.Core.IO;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// implementation that allows for convenient registration of custom
    /// IResource implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Because the <see cref="ResourceHandlerConfigurer"/>
    /// class implements the
    /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// interface, instances of this class that have been exposed in the
    /// scope of an
    /// <see cref="Spring.Context.IApplicationContext"/> will
    /// <i>automatically</i> be picked up by the application context and made
    /// available to the IoC container whenever resolution of IResources is required.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
    /// <seealso cref="Spring.Context.IApplicationContext"/>
    [Serializable]
    public class ResourceHandlerConfigurer : AbstractConfigurer
    {
        private IDictionary resourceHandlers;

        /// <summary>
        /// The IResource implementations, i.e. resource handlers, to register.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The <see cref="System.Collections.IDictionary"/> has the
        /// contains the resource protocol name as the key and type as the value.
        /// The key name can either be a string or an object, in which case
        /// ToString() will be used to obtain the string name.
        /// The value can be the fully qualified name of the IResource
        /// implementation, a string, or
        /// an actual <see cref="System.Type"/> of the IResource class
        ///
        /// </p>
        /// </remarks>
        public IDictionary ResourceHandlers
        {
            set { resourceHandlers = value; }
        }

        /// <summary>
        /// Registers custom IResource implementations.  The supplied
        /// <paramref name="factory"/> is not used since IResourse implementations
        /// are registered with a global <see cref="Spring.Core.IO.ResourceHandlerRegistry"/>
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
            if (resourceHandlers != null)
            {
                foreach (DictionaryEntry entry in resourceHandlers)
                {
                    string protocolName = entry.Key.ToString();
                    Type type = ResolveRequiredType(entry.Value, "value", "custom IResource implementation");
                    ResourceHandlerRegistry.RegisterResourceHandler(protocolName, type);

                }
            }
        }
    }
}

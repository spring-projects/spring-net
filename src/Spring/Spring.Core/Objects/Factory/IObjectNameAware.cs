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

namespace Spring.Objects.Factory
{

    /// <summary>
    /// Interface to be implemented by objects that wish to be aware of their object
    /// name in an <see cref="Spring.Objects.Factory.IObjectFactory"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Note that most objects will choose to receive references to collaborating
    /// objects via respective properties.
    /// </p>
    /// <p>
    /// For a list of all object lifecycle methods, see the
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/> API documentation.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
	public interface IObjectNameAware
    {

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>
        /// The name of the object in the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        string ObjectName
        {
            set;
        }
	}
}

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

using Spring.Core.IO;

#endregion

namespace Spring.Context
{
    /// <summary>
    /// Interface to be implemented by any object that wishes to be notified
    /// of the <see cref="Spring.Core.IO.IResourceLoader"/> (typically the
    /// <see cref="Spring.Context.IApplicationContext"/>) that it runs in.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Note that <see cref="Spring.Core.IO.IResource"/> dependencies can also
    /// be exposed as object properties of type
    /// <see cref="Spring.Core.IO.IResource"/>, populated via strings with
    /// automatic type conversion by the object factory. This obviates the
    /// need for implementing any callback interface just for the purpose of
    /// accessing a specific resource.
    /// </p>
    /// <p>
    /// You typically need an <see cref="Spring.Core.IO.IResourceLoader"/>
    /// when your application object has to access a variety of file resources
    /// whose names are calculated. A good strategy is to make the object use
    /// a default resource loader but still implement the
    /// <see cref="Spring.Context.IResourceLoaderAware"/> interface to allow
    /// for overriding when running in an
    /// <see cref="Spring.Context.IApplicationContext"/>.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
    /// <see cref="Spring.Context.IApplicationContextAware"/>
    /// <see cref="Spring.Objects.Factory.IInitializingObject"/>
    /// <see cref="Spring.Core.IO.IResourceLoader"/>
    public interface IResourceLoaderAware
    {
        /// <summary>
        /// Sets the <see cref="Spring.Core.IO.IResourceLoader"/>
        /// that this object runs in.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Invoked <b>after</b> population of normal objects properties but
        /// before an init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s 
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/>
        /// or a custom init-method. Invoked <b>before</b> setting 
        /// <see cref="Spring.Context.IApplicationContextAware"/>'s
        /// <see cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
        /// property.
        /// </p>
        /// </remarks>
        IResourceLoader ResourceLoader
        {
            set;
        }
    }
}

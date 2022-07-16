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

using Spring.Objects.Factory.Config;

namespace Spring.Context
{
    /// <summary>
    /// Provides the means to configure an application context in addition to
    /// the methods exposed on the
    /// <see cref="Spring.Context.IApplicationContext"/> interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This interface is to be implemented by most (if not all)
    /// <see cref="Spring.Context.IApplicationContext"/> implementations.
    /// </p>
    /// <p>
    /// Configuration and lifecycle methods are encapsulated here to avoid
    /// making them obvious to <see cref="Spring.Context.IApplicationContext"/>
    /// client code.
    /// </p>
    /// <p>
    /// Calling <see cref="System.IDisposable.Dispose()"/> will close this
    /// application context, releasing all resources and locks that the
    /// implementation might hold. This includes disposing all cached
    /// <b>singleton</b> objects.
    /// </p>
    /// <note type="caution">
    /// <see cref="System.IDisposable.Dispose()"/> does <i>not</i> invoke the
    /// attendant <see cref="System.IDisposable.Dispose()"/> on any parent
    /// context.
    /// </note>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="System.IDisposable"/>
    /// <seealso cref="Spring.Context.IApplicationContext"/>
    public interface IConfigurableApplicationContext : IApplicationContext, ILifecycle
    {
        /// <summary>
        /// Return the internal object factory of this application context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Can be used to access specific functionality of the factory.
        /// </p>
        /// <note type="caution">
        /// This is just guaranteed to return an instance that is not
        /// <see langword="null"/> <i>after</i> the context has been refreshed
        /// at least once.
        /// </note>
        /// <note type="caution">
        /// Do not use this to post-process the object factory; singletons
        /// will already have been instantiated. Use an
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        /// to intercept the object factory setup process before objects even
        /// get touched.
        /// </note>
        /// </remarks>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        IConfigurableListableObjectFactory ObjectFactory { get; }

        /// <summary>
        /// Add an
        /// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        /// that will get applied to the internal object factory of this
        /// application context on refresh, before any of the object
        /// definitions are evaluated.
        /// </summary>
        /// <remarks>
        /// <p>
        /// To be invoked during context configuration.
        /// </p>
        /// </remarks>
        /// <param name="objectFactoryPostProcessor">
        /// The factory processor to register.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
        void AddObjectFactoryPostProcessor(
            IObjectFactoryPostProcessor objectFactoryPostProcessor);

        /// <summary>
        /// Load or refresh the persistent representation of the configuration,
        /// which might an XML file, properties file, or relational database schema.
        /// </summary>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// If the configuration cannot be loaded.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object factory could not be initialized.
        /// </exception>
        void Refresh();

        /// <summary>
        /// Sets the parent of this application context.
        /// </summary>
        /// <remarks>
        /// <note>
        /// The parent should <b>not</b> be changed: it should only be set
        /// outside a constructor if it isn't available when an instance of
        /// this class is created.
        /// </note>
        /// </remarks>
        /// <value>
        /// The parent context.
        /// </value>
        new IApplicationContext ParentContext { get; set; }
    }
}

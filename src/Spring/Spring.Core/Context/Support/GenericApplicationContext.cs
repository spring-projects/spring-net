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

using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Context.Support
{
    /// <summary>
    /// Generic ApplicationContext implementation that holds a single internal
    /// <see cref="DefaultListableObjectFactory"/>  instance and does not 
    /// assume a specific object definition format.
    /// </summary>
    /// <remarks>
    /// Implements the <see cref="IObjectDefinitionRegistry"/> interface in order
    /// to allow for aplying any object definition readers to it.
    /// <para>Typical usage is to register a variety of object definitions via the
    /// <see cref="IObjectDefinitionRegistry"/> interface and then call 
    /// <see cref="IConfigurableApplicationContext.Refresh"/> to initialize those
    /// objects with application context semantics (handling 
    /// <see cref="IApplicationContextAware"/>, auto-detecting 
    /// <see cref="IObjectPostProcessor"/> ObjectFactoryPostProcessors, etc).
    /// </para>
    /// <para>In contrast to other IApplicationContext implementations that create a new internal
    /// IObjectFactory instance for each refresh, the internal IObjectFactory of this context
    /// is available right from the start, to be able to register object definitions on it.
    /// <see cref="IConfigurableApplicationContext.Refresh"/> may only be called once</para>
    /// <para>Usage examples</para>
    /// <example>
    /// GenericApplicationContext ctx = new GenericApplicationContext();
    /// // register your objects and object definitions
    /// ctx.RegisterObjectDefinition(...)
    /// ctx.Refresh();
    /// </example>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class GenericApplicationContext : AbstractApplicationContext
    {
        private readonly DefaultListableObjectFactory objectFactory;
        private bool refreshed = false;


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        public GenericApplicationContext()
            : this(null, true, null, new DefaultListableObjectFactory())
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        public GenericApplicationContext(bool caseSensitive)
            : this(null, caseSensitive, null, new DefaultListableObjectFactory())
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory instance to use for this context.</param>
        public GenericApplicationContext(DefaultListableObjectFactory objectFactory)
            : this(null, true, null, objectFactory)
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="parent">The parent application context.</param>
        public GenericApplicationContext(IApplicationContext parent)
            : this(null, true, parent, new DefaultListableObjectFactory())
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="name">The name of the application context.</param>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        /// <param name="parent">The parent application context.</param>
        public GenericApplicationContext(string name, bool caseSensitive, IApplicationContext parent)
            : this(name, caseSensitive, parent, new DefaultListableObjectFactory())
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory to use for this context</param>
        /// <param name="parent">The parent applicaiton context.</param>
        public GenericApplicationContext(DefaultListableObjectFactory objectFactory, IApplicationContext parent)
            : this(null, true, parent, objectFactory)
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="name">The name of the application context.</param>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        /// <param name="parent">The parent application context.</param>
        /// <param name="objectFactory">The object factory to use for this context</param>
        public GenericApplicationContext(string name, bool caseSensitive, IApplicationContext parent, DefaultListableObjectFactory objectFactory)
            : base(name, caseSensitive, parent)
        {
            AssertUtils.ArgumentNotNull(objectFactory, "objectFactory", "ObjectFactory must not be null");
            this.objectFactory = objectFactory;
            this.objectFactory.ParentObjectFactory = base.GetInternalParentObjectFactory();
        }

        /// <summary>
        /// Do nothing operation.  We hold a single internal ObjectFactory and rely on callers
        /// to register objects throug our public methods (or the ObjectFactory's).
        /// </summary>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors encountered while refreshing the object factory.
        /// </exception>
        protected override void RefreshObjectFactory()
        {
            if (refreshed)
            {
                throw new InvalidOperationException(
                    "GenericApplicationContext does not support multiple refresh attempts: just call 'refresh' once");
            }

            refreshed = true;
        }

        /// <summary>
        /// Return the internal object factory of this application context.
        /// </summary>
        /// <value></value>
        public override IConfigurableListableObjectFactory ObjectFactory
        {
            get { return objectFactory; }
        }

        /// <summary>
        /// Gets the underlying object factory of this context, available for 
        /// registering object definitions.
        /// </summary>
        /// <remarks>You need to call <code>Refresh</code> to initialize the
        /// objects factory and its contained objects with application context
        /// semantics (autodecting IObjectFactoryPostProcessors, etc).</remarks>
        /// <value>The internal object factory (as DefaultListableObjectFactory).</value>
        public DefaultListableObjectFactory DefaultListableObjectFactory
        {
            get { return objectFactory; }
        }

        /// <summary>
        /// Determines whether the given object name is already in use within this factory,
        /// i.e. whether there is a local object or alias registered under this name or
        /// an inner object created with this name.
        /// </summary>
        public override bool IsObjectNameInUse(string objectName)
        {
            return objectFactory.IsObjectNameInUse(objectName);
        }
    }
}

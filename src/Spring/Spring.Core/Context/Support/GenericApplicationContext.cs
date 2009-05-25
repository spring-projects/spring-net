#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System;
using Spring.Core.IO;
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
    /// 
    /// </example>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class GenericApplicationContext : AbstractApplicationContext, IObjectDefinitionRegistry
    {
        private DefaultListableObjectFactory objectFactory;

        private bool refreshed = false;


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        public GenericApplicationContext()
        {
            objectFactory = new DefaultListableObjectFactory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        public GenericApplicationContext(bool caseSensitive)
        {
            objectFactory = new DefaultListableObjectFactory(caseSensitive);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory instance to use for this context.</param>
        public GenericApplicationContext(DefaultListableObjectFactory objectFactory)
        {
            AssertUtils.ArgumentNotNull(objectFactory, "objectFactory", "ObjectFactory must not be null");
            this.objectFactory = objectFactory; 
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="parent">The parent application context.</param>
        public GenericApplicationContext(IApplicationContext parent)
        {
            objectFactory = new DefaultListableObjectFactory();
            ParentContext = parent;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="name">The name of the application context.</param>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        /// <param name="parent">The parent application context.</param>
        public GenericApplicationContext(string name, bool caseSensitive, IApplicationContext parent) : this(caseSensitive)
        {
            Name = name;
            ParentContext = parent;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory to use for this context</param>
        /// <param name="parent">The parent applicaiton context.</param>
        public GenericApplicationContext(DefaultListableObjectFactory objectFactory, IApplicationContext parent) : this(objectFactory)
        {
            ParentContext = parent;
        }



        /// <summary>
        /// Gets the parent context, or <see langword="null"/> if there is no
        /// parent context.  Set the parent of this application context also setting
        /// the parent of the interanl ObjectFactory accordingly.
        /// </summary>
        /// <value>The parent context</value>
        /// <returns>
        /// The parent context, or <see langword="null"/>  if there is no
        /// parent.
        /// </returns>
        /// <seealso cref="Spring.Context.IApplicationContext.ParentContext"/>
        public override IApplicationContext ParentContext
        {
            get
            {
                return base.ParentContext;
            }
            set { 
                base.ParentContext = value;
                objectFactory.ParentObjectFactory = GetInternalParentObjectFactory();
            }
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



        #region IObjectDefinitionRegistry Members

        /// <summary>
        /// Returns the
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
        /// for the given object name.
        /// </summary>
        /// <param name="name">The name of the object to find a definition for.</param>
        /// <returns>
        /// The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for
        /// the given name (never null).
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If the object definition cannot be resolved.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public override IObjectDefinition GetObjectDefinition(string name)
        {
            return objectFactory.GetObjectDefinition(name);
        }

        /// <summary>
        /// Register a new object definition with this registry.
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// and <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </summary>
        /// <param name="name">The name of the object instance to register.</param>
        /// <param name="definition">The definition of the object instance to register.</param>
        /// <remarks>
        /// 	<p>
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> and
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object definition is invalid.
        /// </exception>
        public void RegisterObjectDefinition(string name, IObjectDefinition definition)
        {
            objectFactory.RegisterObjectDefinition(name, definition);
        }

        /// <summary>
        /// Given a object name, create an alias. We typically use this method to
        /// support names that are illegal within XML ids (used for object names).
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="theAlias">The alias that will behave the same as the object name.</param>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If the alias is already in use.
        /// </exception>
        public void RegisterAlias(string name, string theAlias)
        {
            objectFactory.RegisterAlias(name, theAlias);
        }

        /// <summary>
        /// Determines whether the given object name is already in use within this factory,
        /// i.e. whether there is a local object or alias registered under this name or
        /// an inner object created with this name.
        /// </summary>
        public bool IsObjectNameInUse(string objectName)
        {
            return objectFactory.IsObjectNameInUse(objectName);
        }

        #endregion
    }
}
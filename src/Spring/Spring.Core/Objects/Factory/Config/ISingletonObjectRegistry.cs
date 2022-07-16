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

using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Interface that defines a registry for shared object instances.
    /// </summary>
    /// <remarks>
    /// Can be implemented by <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// implementations in order to expose their singleton management facility
    /// in a uniform manner.
    /// <para>
    /// The <see cref="IConfigurableObjectFactory"/> interface extends this interface.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface ISingletonObjectRegistry
    {
        /// <summary>
        /// Registers the given existing object as singleton in the object registry,
        /// under the given object name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The given instance is supposed to be fully initialized; the registry
        /// will not perform any initialization callbacks (in particular, it won't
        /// call IInitializingObject's <code>AfterPropertiesSet</code> method).
        /// The given instance will not receive any destruction callbacks
        /// (like IDisposable's <code>Dispose</code> method) either.
        /// </para>
        /// <para>
        /// If running within a full IObjectFactory: Register an object definition
        /// instead of an existing instance if your object is supposed to receive
        /// initialization and/or destruction callbacks.
        /// </para>
        /// <para>
        /// Typically invoked during registry configuration, but can also be used
        /// for runtime registration of singletons. As a consequence, a registry
        /// implementation should synchronize singleton access; it will have to do
        /// this anyway if it supports a BeanFactory's lazy initialization of singletons.
        /// </para>
        /// </remarks>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="singletonObject">The singleton object.</param>
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.RegisterObjectDefinition"/>
        void RegisterSingleton(string objectName, object singletonObject);


        /// <summary>
        /// Return the (raw) singleton object registered under the given name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only checks already instantiated singletons; does not return an Object
        /// for singleton object definitions which have not been instantiated yet.
        /// </para>
        /// <para>
        /// The main purpose of this method is to access manually registered singletons
        /// <see cref="RegisterSingleton"/>. Can also be used to access a singleton
        /// defined by an object definition that already been created, in a raw fashion.
        /// </para>
        /// </remarks>
        /// <param name="objectName">Name of the object to look for.</param>
        /// <returns>the registered singleton object, or <code>null</code> if none found</returns>
        /// <see cref="IConfigurableListableObjectFactory.GetObjectDefinition(string)"/>
        object GetSingleton(string objectName);


        /// <summary>
        /// Check if this registry contains a singleton instance with the given name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only checks already instantiated singletons; does not return <code>true</code>
        /// for singleton bean definitions which have not been instantiated yet.
        /// </para>
        /// <para>
        /// The main purpose of this method is to check manually registered singletons
        /// <see cref="RegisterSingleton"/>.  Can also be used to check whether a
        /// singleton defined by an object definition has already been created.
        /// </para>
        /// <para>
        /// To check whether an object factory contains an object definition with a given name,
        /// use ListableBeanFactory's <code>ContainsObjectDefinition</code>. Calling both
        /// <code>ContainsObjectDefinition</code> and <code>ContainsSingleton</code> answers
        /// whether a specific object factory contains an own object with the given name.
        /// </para>
        /// <para>
        /// Use IObjectFactory's <code>ContainsObject</code> for general checks whether the
        /// factory knows about an object with a given name (whether manually registered singleton
        /// instance or created by bean definition), also checking ancestor factories.
        /// </para>
        /// </remarks>
        /// <param name="objectName">Name of the object to look for.</param>
        /// <returns>
        /// 	<c>true</c> if this bean factory contains a singleton instance with the given name; otherwise, <c>false</c>.
        /// </returns>
        /// <see cref="RegisterSingleton"/>
        /// <see cref="Spring.Objects.Factory.IListableObjectFactory.ContainsObjectDefinition"/>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.ContainsObject"/>
        bool ContainsSingleton(string objectName);

        /// <summary>
        /// Gets the names of singleton objects registered in this registry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only checks already instantiated singletons; does not return names
        /// for singleton bean definitions which have not been instantiated yet.
        /// </para>
        /// <para>
        /// The main purpose of this method is to check manually registered singletons
        /// <see cref="RegisterSingleton"/>. Can also be used to check which
        /// singletons defined by an object definition have already been created.
        /// </para>
        /// </remarks>
        /// <value>The list of names as String array (never <code>null</code>).</value>
        /// <see cref="RegisterSingleton"/>
        /// <see cref="IObjectDefinitionRegistry.GetObjectDefinitionNames()"/>
        /// <see cref="IListableObjectFactory.GetObjectDefinitionNames()"/>
        IList<string> SingletonNames
        {
            get;
        }

        /// <summary>
        /// Gets the number of singleton beans registered in this registry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only checks already instantiated singletons; does not count
        /// singleton object definitions which have not been instantiated yet.
        /// </para>
        /// <para>
        /// The main purpose of this method is to check manually registered singletons
        /// <see cref="RegisterSingleton"/>.  Can also be used to count the number of
        /// singletons defined by an object definition that have already been created.
        /// </para>
        /// </remarks>
        /// <value>The number of singleton objects.</value>
        /// <see cref="RegisterSingleton"/>
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.ObjectDefinitionCount"/>
        /// <see cref="Spring.Objects.Factory.IListableObjectFactory.ObjectDefinitionCount"/>
        int SingletonCount
        {
            get;
        }
    }
}

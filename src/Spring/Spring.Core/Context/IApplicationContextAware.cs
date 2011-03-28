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

namespace Spring.Context
{
    /// <summary>
    /// To be implemented by any object that wishes to be notified
    /// of the <see cref="Spring.Context.IApplicationContext"/> that it runs in.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Implementing this interface makes sense when an object requires access
    /// to a set of collaborating objects. Note that configuration via object
    /// references is preferable to implementing this interface just for object
    /// lookup purposes.
    /// </p>
    /// <p>
    /// This interface can also be implemented if an object needs access to
    /// file resources, i.e. wants to call
    /// <see cref="Spring.Core.IO.IResourceLoader.GetResource"/>, or access to
    /// the <see cref="Spring.Context.IMessageSource"/>. However, it is
    /// preferable to implement the more specific
    /// <see cref="Spring.Context.IResourceLoaderAware"/>
    /// interface to receive a reference to the
    /// <see cref="Spring.Context.IMessageSource"/> object in that scenario.
    /// </p>
    /// <p>
    /// Note that <see cref="Spring.Core.IO.IResource"/> dependencies can also
    /// be exposed as object properties of the
    /// <see cref="Spring.Core.IO.IResource"/> type, populated via strings with
    /// automatic type conversion performed by an object factory. This obviates
    /// the need for implementing any callback interface just for the purpose
    /// of accessing a specific file resource.
    /// </p>
    /// <p>
    /// <see cref="Spring.Context.Support.ApplicationObjectSupport"/>
    /// is a convenience implementation of this interface for your
    /// application objects.
    /// </p>
    /// <p>
    /// For a list of all object lifecycle methods, see the overview for the 
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
    /// <see cref="Spring.Objects.Factory.IInitializingObject"/>
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/>
    public interface IApplicationContextAware
    {
    	/// <summary>
    	/// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
    	/// object runs in.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// Normally this call will be used to initialize the object.
    	/// </p>
    	/// <p>
    	/// Invoked after population of normal object properties but before an
    	/// init callback such as
    	/// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    	/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
    	/// or a custom init-method. Invoked after the setting of any
    	/// <see cref="Spring.Context.IResourceLoaderAware"/>'s
    	/// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
    	/// property.
    	/// </p>
    	/// </remarks>
    	/// <exception cref="Spring.Context.ApplicationContextException">
    	/// In the case of application context initialization errors.
    	/// </exception>
    	/// <exception cref="Spring.Objects.ObjectsException">
    	/// If thrown by any application context methods.
    	/// </exception>
    	/// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
    	IApplicationContext ApplicationContext { set; }
    }
}

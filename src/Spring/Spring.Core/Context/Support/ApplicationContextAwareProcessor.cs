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

using System.Runtime.Remoting;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
	/// implementation that passes the application context to object that
	/// implement the
	/// <see cref="Spring.Context.IApplicationContextAware"/>,
	/// <see cref="Spring.Context.IMessageSourceAware"/>, and
	/// <see cref="Spring.Context.IResourceLoaderAware"/> interfaces. 
	/// </summary>
	/// <remarks>
	/// <p>
	/// If an object's class implements more than one of the
	/// <see cref="Spring.Context.IApplicationContextAware"/>,
	/// <see cref="Spring.Context.IMessageSourceAware"/>, and
	/// <see cref="Spring.Context.IResourceLoaderAware"/> interfaces, then the
	/// order in which the interfaces are satisfied is as follows...
	/// <list type="bullet">
	/// <item><description>
	/// <see cref="Spring.Context.IResourceLoaderAware"/>
	/// </description></item>
	/// <item><description>
	/// <see cref="Spring.Context.IMessageSourceAware"/>
	/// </description></item>
	/// <item><description>
	/// <see cref="Spring.Context.IApplicationContextAware"/>
	/// </description></item>
	/// </list>
	/// </p>
	/// <p>
	/// Application contexts will automatically register this with their
	/// underlying object factory. Applications should thus never need to use
	/// this class directly.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class ApplicationContextAwareProcessor : IObjectPostProcessor
	{
		private IApplicationContext _applicationContext;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.ApplicationContextAwareProcessor"/> class.
		/// </summary>
		/// <param name="applicationContext">
		/// The <see cref="Spring.Context.IApplicationContext"/> that this
		/// instance will work with.
		/// </param>
		public ApplicationContextAwareProcessor(
			IApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
		}

		/// <summary>
		/// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
		/// to the given new object instance <i>before</i> any object
		/// initialization callbacks.
		/// </summary>
		/// <param name="obj">
		/// The new object instance.
		/// </param>
		/// <param name="objectName">
		/// The name of the object.
		/// </param>
		/// <returns>
		/// The the object instance to use, either the original or a wrapped one.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization"/>
		public object PostProcessAfterInitialization(object obj, string objectName)
		{
			return obj;
		}

		/// <summary>
		/// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> to the
		/// given new object instance <i>after</i> any object initialization
		/// callbacks.
		/// </summary>
		/// <param name="obj">
		/// The new object instance.
		/// </param>
		/// <param name="name">
		/// The name of the object.
		/// </param>
		/// <returns>
		/// The object instance to use, either the original or a wrapped one.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization"/>
		public object PostProcessBeforeInitialization(object obj, string name)
		{
			if(!RemotingServices.IsTransparentProxy(obj)) 
			{
				if (typeof (IResourceLoaderAware).IsInstanceOfType(obj))
				{
					((IResourceLoaderAware) obj).ResourceLoader
						= _applicationContext;
				}
				if (typeof (IMessageSourceAware).IsInstanceOfType(obj))
				{
					((IMessageSourceAware) obj).MessageSource
						= _applicationContext;
				}
				if (typeof (IApplicationContextAware).IsInstanceOfType(obj))
				{
					((IApplicationContextAware) obj).ApplicationContext
						= _applicationContext;
				}
			}
			return obj;
		}
	}
}
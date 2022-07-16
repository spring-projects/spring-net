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

#region Imports

using Spring.Core.IO;
using Spring.Objects.Events;
using Spring.Objects.Factory;

#endregion

namespace Spring.Context
{
	/// <summary>
	/// The central interface to Spring.NET's IoC container.
	/// </summary>
	/// <remarks>
	/// <p>
	/// <see cref="Spring.Context.IApplicationContext"/> implementations
	/// provide:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// Object factory functionality inherited from the
	/// <see cref="Spring.Objects.Factory.IListableObjectFactory"/>
	/// and <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>
	/// interfaces.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The ability to resolve messages, supporting internationalization.
	/// Inherited from the <see cref="Spring.Context.IMessageSource"/>
	/// interface.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The ability to load file resources in a generic fashion.
	/// Inherited from the <see cref="Spring.Core.IO.IResourceLoader"/>
	/// interface.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Acts an an event registry for supporting loosely coupled eventing
	/// between objecs. Inherited from the
	/// <see cref="Spring.Objects.Events.IEventRegistry"/> interface.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The ability to raise events related to the context lifecycle. Inherited
	/// from the <see cref="Spring.Context.IApplicationEventPublisher"/>
	/// interface.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Inheritance from a parent context. Definitions in a descendant context
	/// will always take priority.
	/// </description>
	/// </item>
	/// </list>
	/// </p>
	/// <p>
	/// In addition to standard object factory lifecycle capabilities,
	/// <see cref="Spring.Context.IApplicationContext"/> implementations need
	/// to detect
	/// <see cref="Spring.Context.IApplicationContextAware"/>,
	/// <see cref="Spring.Objects.Events.IEventRegistryAware"/>, and
	/// <see cref="Spring.Context.IMessageSourceAware"/> objects and supply
	/// their attendant dependencies accordingly.
	/// </p>
	/// <p>
	/// This interface is the central client interface in Spring.NET's IoC
	/// container implementation. As such it does inherit a quite sizeable
	/// number of interfaces; implementations are strongly encouraged to use
	/// composition to satisfy each of the inherited interfaces (where
	/// appropriate of course).
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <seealso cref="Spring.Context.Support.AbstractApplicationContext"/>
	/// <seealso cref="Spring.Context.Support.XmlApplicationContext"/>
	/// <seealso cref="Spring.Context.Support.DelegatingMessageSource"/>
	/// <seealso cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>
	public interface IApplicationContext
		: IListableObjectFactory, IHierarchicalObjectFactory, IMessageSource,
			IApplicationEventPublisher, IResourceLoader, IEventRegistry, IDisposable
	{
		/// <summary>
		/// Raised in response to an application context event.
		/// </summary>
		event ApplicationEventHandler ContextEvent;

		/// <summary>
		/// Returns the date and time this context was loaded.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is to be set immediately after an 
		/// <see cref="Spring.Context.IApplicationContext"/> has been
		/// instantiated and its configuration has been loaded. Implementations
		/// are permitted to update this value if the context is reset or
		/// refreshed in some way.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The <see cref="System.DateTime"/> representing when this context
		/// was loaded.
		/// </returns>
		/// <seealso cref="Spring.Context.IConfigurableApplicationContext.Refresh"/>
		DateTime StartupDate { get; }

		/// <summary>
		/// Gets the parent context, or <see langword="null"/> if there is no
		/// parent context.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If the parent context is <see langword="null"/>, then this context
		/// is the root of any context hierarchy.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The parent context, or <see langword="null"/>  if there is no
		/// parent.
		/// </returns>
		IApplicationContext ParentContext { get; }

		/// <summary>
		/// Gets and sets a name for this context.
		/// </summary>
		/// <returns>
		/// A name for this context.
		/// </returns>
		string Name { get; set; }

	}
}

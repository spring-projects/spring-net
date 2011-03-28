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

namespace Spring.Context {

	/// <summary>
	/// To be implemented by any object that wishes to be notified
	/// of the <see cref="Spring.Context.IMessageSource"/> associated with it.
	/// </summary>
	/// <remarks>
	/// <p>
	/// In the current implementation, the
	/// <see cref="Spring.Context.IMessageSource"/> will typically be the
	/// associated <see cref="Spring.Context.IApplicationContext"/> that
	/// spawned the implementing object.
	/// </p>
	/// <p>
	/// The <see cref="Spring.Context.IMessageSource"/> can usually also be
	/// passed on as an object reference to arbitrary object properties or
	/// constructor arguments, because a
	/// <see cref="Spring.Context.IMessageSource"/> is typically defined as an
	/// object with the well known name <c>"messageSource"</c> in the
	/// associated application context.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.IInitializingObject"/>
	public interface IMessageSourceAware 
    {
		/// <summary>
		/// Sets the <see cref="Spring.Context.IMessageSource"/> associated
		/// with this object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked <b>after</b> population of normal object properties but
		/// <b>before</b> an initializing callback such as the
		/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		/// method of the
		/// <see cref="Spring.Objects.Factory.IInitializingObject"/> interface
		/// or a custom init-method.
		/// </p>
		/// <p>
		/// It is also invoked <b>before</b> the
		/// <see cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
		/// property of any
		/// <see cref="Spring.Context.IApplicationContextAware"/>
		/// implementation.
		/// </p>
		/// </remarks>
		/// <value>
		/// The <see cref="Spring.Context.IMessageSource"/> associated
		/// with this object.
		/// </value>
		IMessageSource MessageSource 
		{
			set;
		}
	}
}

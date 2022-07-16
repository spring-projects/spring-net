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

namespace Spring.Objects.Events
{
	/// <summary>
	/// To be implemented by any object that wishes to receive a reference to
	/// an <see cref="Spring.Objects.Events.IEventRegistry"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This interface only applies to objects that have been instantiated
	/// within the context of an
	/// <see cref="Spring.Context.IApplicationContext"/>. This interface does
	/// not typically need to be implemented by application code, but is rather
	/// used by classes internal to Spring.NET.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack</author>
	/// <author>Rick Evans</author>
	public interface IEventRegistryAware
	{
		/// <summary>
		/// Set the <see cref="Spring.Objects.Events.IEventRegistry"/>
		/// associated with the
		/// <see cref="Spring.Context.IApplicationContext"/> that created this
		/// object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This property will be set by the relevant
		/// <see cref="Spring.Context.IApplicationContext"/> after all of this
		/// object's dependencies have been resolved. This object can use the
		/// supplied <see cref="Spring.Objects.Events.IEventRegistry"/>
		/// immediately to publish or subscribe to one or more events.
		/// </p>
		/// </remarks>
		IEventRegistry EventRegistry { set; }
	}
}

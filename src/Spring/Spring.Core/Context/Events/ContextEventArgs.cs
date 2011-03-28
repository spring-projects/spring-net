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

using System.Globalization;

namespace Spring.Context.Events
{
	/// <summary>
	/// Event object sent to listeners registered with an
	/// <see cref="Spring.Context.IApplicationContext"/> to inform them of
	/// context lifecycle events.
	/// </summary>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.IApplicationContext"/>
	/// <seealso cref="Spring.Context.IApplicationEventListener"/>
	/// <seealso cref="Spring.Context.EventListenerAttribute"/>
	public class ContextEventArgs : ApplicationEventArgs
	{
		/// <summary>
		/// The various context event types.
		/// </summary>
		public enum ContextEvent
		{
			/// <summary>
			/// The event type when the context is refreshed or created.
			/// </summary>
			Refreshed,

			/// <summary>
			/// The event type when the context is closed.
			/// </summary>
			Closed
		} ;

		private readonly ContextEvent _contextEvent;

		/// <summary>
		/// Creates a new instance of the ContextEventArgs class to represent the
		/// supplied context event.
		/// </summary>
		/// <param name="e">The type of context event.</param>
		public ContextEventArgs(ContextEvent e)
		{
			_contextEvent = e;
		}

		/// <summary>
		/// The event type.
		/// </summary>
		public ContextEvent Event
		{
			get { return _contextEvent; }
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} [{1}]", GetType().Name, Event);
		}
	}

    /// <summary>
    /// Event object sent to listeners registered with an
    /// <see cref="Spring.Context.IApplicationContext"/> to inform them of
    /// context <see cref="ContextEventArgs.ContextEvent.Refreshed"/> lifecycle event.
    /// </summary>
    public class ContextRefreshedEventArgs : ContextEventArgs
    {
        public ContextRefreshedEventArgs() : base(ContextEvent.Refreshed)
        {
        }
    }

    /// <summary>
    /// Event object sent to listeners registered with an
    /// <see cref="Spring.Context.IApplicationContext"/> to inform them of
    /// context <see cref="ContextEventArgs.ContextEvent.Closed"/> lifecycle event.
    /// </summary>
    public class ContextClosedEventArgs : ContextEventArgs
    {
        public ContextClosedEventArgs() : base(ContextEvent.Closed)
        {
        }
    }
}
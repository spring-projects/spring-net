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

using System.Reflection;
using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Describes an event handler for an object instance.
	/// </summary>
	/// <author>Rick Evans</author>
	public class InstanceEventHandlerValue : AbstractWiringEventHandlerValue
	{
		#region Constants

		private static readonly BindingFlags InstanceMethodFlags =
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.IgnoreCase | BindingFlags.Static;

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.InstanceEventHandlerValue"/> class.
		/// </summary>
		public InstanceEventHandlerValue()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.InstanceEventHandlerValue"/> class.
		/// </summary>
		/// <param name="source">
		/// The object (possibly unresolved) that is exposing the event.
		/// </param>
		/// <param name="methodName">
		/// The name of the method on the handler that is going to handle the event.
		/// </param>
		public InstanceEventHandlerValue(object source, string methodName)
			: base(source, methodName)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the event handler.
		/// </summary>
		/// <param name="instance">
		/// The instance that is registering for the event notification.
		/// </param>
		/// <param name="info">
		/// Event metadata about the event.
		/// </param>
		/// <returns>
		/// The event handler.
		/// </returns>
		protected override Delegate GetHandler(object instance, EventInfo info)
		{
			MethodInfo methodMeta = ResolveHandlerMethod(
				ReflectionUtils.TypeOfOrType(instance),
				info.EventHandlerType,
				InstanceEventHandlerValue.InstanceMethodFlags);
			Delegate callback = null;
			if (methodMeta.IsStatic)
			{
				// case insensitive binding to a static method on an (irrelevant) instance
				callback = Delegate.CreateDelegate(
					info.EventHandlerType,
					methodMeta);
			}
			else
			{
				// case insensitive binding to an instance method on an instance
				callback =
					Delegate.CreateDelegate(
						info.EventHandlerType,
						instance,
						MethodName,
						true);
			}
			return callback;
		}

		#endregion
	}
}

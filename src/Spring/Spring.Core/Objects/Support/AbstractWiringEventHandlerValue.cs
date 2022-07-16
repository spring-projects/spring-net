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

using System.Globalization;
using System.Reflection;
using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Base class for all <see cref="Spring.Objects.IEventHandlerValue"/>
	/// implemenations that actually perform event wiring.
	/// </summary>
	/// <author>Rick Evans</author>
	public abstract class AbstractWiringEventHandlerValue : AbstractEventHandlerValue
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.AbstractWiringEventHandlerValue"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		protected AbstractWiringEventHandlerValue()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.AbstractWiringEventHandlerValue"/> class.
		/// </summary>
		/// <param name="source">
		/// The object (possibly unresolved) that is exposing the event.
		/// </param>
		/// <param name="methodName">
		/// The name of the method on the handler that is going to handle the event.
		/// </param>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		protected AbstractWiringEventHandlerValue(object source, string methodName)
			: base(source, methodName)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Wires up the specified handler to the named event on the
		/// supplied event source.
		/// </summary>
		/// <param name="source">
		/// The object (an object instance, a <see cref="System.Type"/>, etc)
		/// exposing the named event.
		/// </param>
		/// <param name="handler">
		/// The handler for the event (an object instance, a
		/// <see cref="System.Type"/>, etc).
		/// </param>
		public override void Wire(object source, object handler)
		{
			Type sourceType = ReflectionUtils.TypeOfOrType(source);
			EventInfo eventInfo = sourceType.GetEvent(EventName);
			Delegate callback = GetHandler(handler, eventInfo);
			eventInfo.AddEventHandler(source, callback);
		}

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
		protected abstract Delegate GetHandler(object instance, EventInfo info);

		/// <summary>
		/// Resolves the method metadata that describes the method that is to be used
		/// as the argument to a delegate constructor.
		/// </summary>
		/// <param name="handlerType">
		/// The <see cref="System.Type"/> exposing the method.
		/// </param>
		/// <param name="delegateType">
		/// The <see cref="System.Type"/> of the delegate (e.g. System.EventHandler).
		/// </param>
		/// <param name="flags">
		/// The custom binding flags to use when searching for the method.
		/// </param>
		/// <returns>The method metadata.</returns>
		/// <exception cref="Spring.Objects.FatalObjectException">
		/// If the method could not be found.
		/// </exception>
		protected virtual MethodInfo ResolveHandlerMethod(
			Type handlerType, Type delegateType, BindingFlags flags)
		{
			MethodInfo method = handlerType.GetMethod(
				MethodName,
				flags,
				Type.DefaultBinder,
				CallingConventions.Standard,
				// cache this? for EventHandler types, we're always gonna get the same parameters
				new DelegateInfo(delegateType).GetParameterTypes(),
				null);

			#region Sanity Check

			if (method == null)
			{
				throw new FatalObjectException(string.Format(
					CultureInfo.InvariantCulture,
					"The '{0}' method could not be found on this object [{1}]",
					MethodName, handlerType));
			}

			#endregion

			return method;
		}

		#endregion
	}
}

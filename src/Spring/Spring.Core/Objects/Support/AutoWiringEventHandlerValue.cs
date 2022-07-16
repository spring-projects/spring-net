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
using System.Reflection;
using Common.Logging;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Describes an <see cref="Spring.Objects.IEventHandlerValue"/> implementation
	/// that autowires events to handler methods.
	/// </summary>
	/// <author>Rick Evans</author>
	public class AutoWiringEventHandlerValue : AbstractEventHandlerValue
	{
		#region Constants

		private const string EventNamePlaceHolder = "${event}";

		private const string DefaultMethodPrefix = "On";

		private const string DefaultMethodName = DefaultMethodPrefix + EventNamePlaceHolder;

		private static readonly ILog log
			= LogManager.GetLogger(typeof (AutoWiringEventHandlerValue));

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.AutoWiringEventHandlerValue"/> class.
		/// </summary>
		public AutoWiringEventHandlerValue()
		{
			MethodName = DefaultMethodName;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the method that is going to handle the event.
		/// </summary>
		public override string MethodName
		{
			get { return base.MethodName; }
			set { base.MethodName = StringUtils.HasText(value) ? value.Trim() : DefaultMethodName; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Wires up the specified handler to the named event on the supplied event source.
		/// </summary>
		/// <param name="source">
		/// The object (an object instance, a <see cref="System.Type"/>, etc)
		/// exposing the named event.
		/// </param>
		/// <param name="handler">
		/// The handler for the event (an object instance, a <see cref="System.Type"/>,
		/// etc).
		/// </param>
		public override void Wire(object source, object handler)
		{
			AssertUtils.ArgumentNotNull(source, "source");
			AssertUtils.ArgumentNotNull(handler, "handler");
			// simply delegate so that after wiring, state is wiped clean...
			AutoWirer wirer = new AutoWirer(source, EventName, handler, MethodName);
			wirer.Wire();
		}

		#endregion

		#region Inner Class : AutoWirer

		/// <summary>
		/// Performs the matching up of handler methods to one or more source events.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This class merely marshals the matching of handler methods to the events exposed
		/// by an event source, and then delegates to a concrete
		/// <see cref="Spring.Objects.IEventHandlerValue"/> implementation (such as
		/// <see cref="Spring.Objects.Support.InstanceEventHandlerValue"/> or
		/// <see cref="Spring.Objects.Support.StaticEventHandlerValue"/>) to do the heavy lifting of
		/// actually wiring a handler method to an event.
		/// </p>
		/// <p>
		/// Note : the order in which handler's are wired up to events is non-deterministic.
		/// </p>
		/// </remarks>
		private sealed class AutoWirer
		{
			#region Constructor (s) / Destructor

			/// <summary>
			/// Creates a new instance of the
			/// <see cref="Spring.Objects.Support.AutoWiringEventHandlerValue.AutoWirer"/> class.
			/// </summary>
			/// <param name="source">
			/// The object exposing the event (s) being wired up.
			/// </param>
			/// <param name="eventName">
			/// The name of the event that is being wired up.
			/// </param>
			/// <param name="handler">
			/// The object exposing the method (s) being wired to the event.
			/// </param>
			/// <param name="methodName">
			/// The name of the method that is going to handle the event.
			/// </param>
			public AutoWirer(
				object source, string eventName, object handler, string methodName)
			{
				Source = source;
				EventName = eventName;
				Handler = handler;
				MethodName = methodName;
			}

			#endregion

			#region Methods

			/// <summary>
			/// Wires up events on the source to methods exposed on the handler.
			/// </summary>
			public void Wire()
			{
				Type sourceType = ReflectionUtils.TypeOfOrType(Source);
				// create the criteria for the event search...
				ICriteria criteria = new RegularExpressionEventNameCriteria(EventName);
				// and grab the events that satisfy the criteria...
				BindingFlags eventFlags = BindingFlags.Instance | BindingFlags.Static
					| BindingFlags.Public;
				MemberInfo[] events = sourceType.FindMembers(
					MemberTypes.Event,
					eventFlags,
					new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
					criteria);
				// and for each event that satisfied the criteria...
				foreach (EventInfo evt in events)
				{
					WireEvent(evt);
				}
			}

			/// <summary>
			/// Wires up the supplied event to any handler methods that match the event
			/// signature.
			/// </summary>
			/// <param name="theEvent">The event being wired up.</param>
			private void WireEvent(EventInfo theEvent)
			{
				// grab some info (such as the delegate's method signature) about the event
				DelegateInfo eventDelegate = new DelegateInfo(theEvent);
				// if the method name needs to be customised on a per event basis, do so
				string customMethodName = GetMethodNameCustomisedForEvent(theEvent.Name);

				// create the criteria for the handler method search...
				ComposedCriteria methodCriteria = new ComposedCriteria();
				// a candidate handlers method name must match the custom method name
				methodCriteria.Add(new RegularExpressionMethodNameCriteria(customMethodName));
				// the return Type of a candidate handlers method must be the same as the return type of the event
				methodCriteria.Add(new MethodReturnTypeCriteria(eventDelegate.GetReturnType()));
				// a candidate handlers method parameters must match the event's parameters
				methodCriteria.Add(new MethodParametersCriteria(eventDelegate.GetParameterTypes()));

				// and grab the methods that satisfy the criteria...
				BindingFlags methodFlags = BindingFlags.Instance | BindingFlags.Static
					| BindingFlags.NonPublic | BindingFlags.Public;
				MemberInfo[] methods = HandlerType.FindMembers(
					MemberTypes.Method,
					methodFlags,
					new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
					methodCriteria);

				// and for each method that satisfied the criteria...
				foreach (MethodInfo method in methods)
				{
					#region Instrumentation

					if (log.IsDebugEnabled)
					{
						log.Debug(string.Format(
							CultureInfo.InvariantCulture,
							"Wiring up this method '{0}' to this event '{1}'",
							method.Name,
							theEvent.Name));
					}

					#endregion

					IEventHandlerValue myHandler = method.IsStatic ?
						new StaticEventHandlerValue() :
						(IEventHandlerValue) new InstanceEventHandlerValue();
					myHandler.EventName = theEvent.Name;
					myHandler.MethodName = method.Name;
					myHandler.Wire(Source, Handler);
				}
			}

			/// <summary>
			/// Only replaces the <b>first</b> occurrence of the placeholder.
			/// </summary>
			/// <param name="eventName">The event whose name is going to be used.</param>
			/// <returns>
			/// The method name customised for the name of the supplied event.
			/// </returns>
			private string GetMethodNameCustomisedForEvent(string eventName)
			{
				string methodName = MethodName;
				if (MethodName.IndexOf(EventNamePlaceHolder) >= 0)
				{
					methodName = MethodName.Replace(EventNamePlaceHolder, eventName);
				}
				return methodName;
			}

			#endregion

			#region Properties

			/// <summary>
			/// The object exposing the event (s) being wired up.
			/// </summary>
			private object Source
			{
				get { return _source; }
				set { _source = value; }
			}

			/// <summary>
			/// The object exposing the method (s) being wired to an event source.
			/// </summary>
			private object Handler
			{
				get { return _handler; }
				set { _handler = value; }
			}

			/// <summary>
			/// The <see cref="Type"/> of the object that is handling any events.
			/// </summary>
			private Type HandlerType
			{
				get
				{
					if (_handlerType == null)
					{
						_handlerType = ReflectionUtils.TypeOfOrType(Handler);
					}
					return _handlerType;
				}
			}

			/// <summary>
			/// The name of the method that is going to handle the event.
			/// </summary>
			private string MethodName
			{
				get { return _methodName; }
				set { _methodName = value; }
			}

			/// <summary>
			/// The name of the event that is being wired up.
			/// </summary>
			private string EventName
			{
				get { return _eventName; }
				set { _eventName = value; }
			}

			#endregion

			#region Fields

			private object _source;
			private object _handler;
			private string _methodName;
			private string _eventName;
			private Type _handlerType;

			#endregion
		}

		#endregion
	}
}

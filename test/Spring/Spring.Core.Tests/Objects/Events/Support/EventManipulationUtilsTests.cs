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

#region Imports

using System;
using System.Reflection;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Events.Support.Tests
{
	/// <summary>
	/// Unit tests for the EventManipulationUtils class.
	/// </summary>
	[TestFixture]
	public sealed class EventManipulationUtilsTests
	{
		[Test]
		public void FoundEventHandlers()
		{
			Type pubType = typeof (SimpleEventPublisher);
			EventInfo[] events = pubType.GetEvents();

			foreach (EventInfo currentEvent in events)
			{
				Type eventHandlerType = currentEvent.EventHandlerType;
				MethodInfo invoke = eventHandlerType.GetMethod("Invoke");
				Assert.IsNotNull(EventManipulationUtils.GetMethodInfoMatchingSignature(invoke, typeof (SimpleEventSubscriber)));
			}
		}

		[Test]
		public void FoundSomeEventHandlers()
		{
			Type pubType = typeof (SimpleEventPublisher);
			EventInfo[] events = pubType.GetEvents();
			foreach (EventInfo currentEvent in events)
			{
				Type eventHandlerType = currentEvent.EventHandlerType;
				MethodInfo invoke = eventHandlerType.GetMethod("Invoke");
				if (currentEvent.Name == "MyFirstEvent" || currentEvent.Name == "MySecondEvent")
				{
					Assert.IsNotNull(EventManipulationUtils.GetMethodInfoMatchingSignature(invoke, typeof (SomeEventSubscriber)));
				}
				else
				{
					Assert.IsNull(EventManipulationUtils.GetMethodInfoMatchingSignature(invoke, typeof (SomeEventSubscriber)));
				}
			}
		}

		[Test]
		public void FoundNoHandlers()
		{
			Type pubType = typeof (SimpleEventPublisher);
			EventInfo[] events = pubType.GetEvents();

			foreach (EventInfo currentEvent in events)
			{
				Type eventHandlerType = currentEvent.EventHandlerType;
				MethodInfo invoke = eventHandlerType.GetMethod("Invoke");
				Assert.IsNull(EventManipulationUtils.GetMethodInfoMatchingSignature(invoke, typeof (NoEventSubscriber)));
			}
		}

		#region Helper Classes

		internal delegate void Delegate1(object source, string name);

		internal delegate string Delegate2(string name, bool flag);

		internal delegate bool Delegate3(string name, bool flag);

		internal class SimpleEventPublisher
		{
#pragma warning disable 67
			public event Delegate1 MyFirstEvent;
			public event Delegate2 MySecondEvent;
			public event Delegate3 MyThirdEvent;
#pragma warning restore 67
        }

		internal class SimpleEventSubscriber
		{
			public void MyFirstEvent_Handler(object source, string name)
			{
			}

			public string MySecondEvent_Handler(string name, bool flag)
			{
				return String.Empty;
			}

			public bool MyThirdEvent_Handler(string name, bool flag)
			{
				return false;
			}
		}

		internal class NoEventSubscriber
		{
			public void MyFirstEvent_Handler(object source, string name, bool flag)
			{
			}

			public void MySecondEvent_Handler(string name, bool flag)
			{
			}

			public string MyThirdEvent_Handler(string name)
			{
				return String.Empty;
			}
		}

		internal class SomeEventSubscriber
		{
			public void MyFirstEvent_Handler(object source, string name)
			{
			}

			public string MySecondEvent_Handler(string name, bool flag)
			{
				return String.Empty;
			}
		}

		#endregion
	}
}
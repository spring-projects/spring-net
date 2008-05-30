#region License

/* 
 * Copyright © 2002-2006 the original author or authors. 
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

using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.IocQuickStart.EventRegistry
{
	/// <summary>
	/// Small example application showing how objects can publish their events
	/// to an IApplicationContex.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The example then goes on to illustrate how subscribers can subscribe to
	/// any events by notifying an IApplicationContext instance. The context
	/// will only wire events and event handlers if they have compatible
	/// signatures.
	/// </p>
	/// </remarks>
	public sealed class Program
	{
		/// <summary>
		/// In this example, the subscriber is subscribing by publisher type. 
		/// </summary>
		[STAThread]
		public static void Main()
		{
			try
			{
                // Retrieve context defined in the spring/context section of 
                // the standard .NET configuration file.
				using (IApplicationContext ctx = ContextRegistry.GetContext())
				{
					// gets the publisher from the application context...
					MyEventPublisher publisher = (MyEventPublisher) ctx.GetObject("MyEventPublisher");
					// publishes events to the context...
					ctx.PublishEvents(publisher);

					// gets first instance of subscriber...
					MyEventSubscriber subscriber = (MyEventSubscriber) ctx.GetObject("MyEventSubscriber");
					// gets second instance of subscriber...
					MyEventSubscriber subscriber2 = (MyEventSubscriber) ctx.GetObject("MyEventSubscriber");
					// subscribes the first instance to any events published by the MyEventPublisher type...
					ctx.Subscribe(subscriber, typeof (MyEventPublisher));

					Console.WriteLine("Publisher name: " + publisher.PublisherName);
					// must be false for both subscribers as no event has yet been raised...
					Console.WriteLine("Subscriber 1 Event Handled: " + subscriber.EventHandled);
					Console.WriteLine("Subscriber 2 Event Handled: " + subscriber2.EventHandled);

					// raises a publisher event...
					publisher.ClientMethodThatTriggersEvent1();

					// must be true (subscribed to any events)...
					Console.WriteLine("Subscriber 1 Event Handled: " + subscriber.EventHandled);
					// must be false (did not subscribe to any events)...
					Console.WriteLine("Subscriber 2 Event Handled: " + subscriber2.EventHandled);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
                Console.WriteLine();
				Console.WriteLine("--- hit <return> to quit ---");
				Console.ReadLine();	
			}
		}
	}
}
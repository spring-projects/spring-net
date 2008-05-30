#region License

/*
 * Copyright 2002-2006 the original author or authors.
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

#endregion

namespace Spring.IocQuickStart.EventRegistry
{
	public class MyEventSubscriber
	{
		private bool _eventHandled = false;

		public MyEventSubscriber()
		{
		}

		public void HandleClientEvents(object sender, MyClientEventArgs args)
		{
			Console.WriteLine("HandleClientEvents handler in subscriber handled event with args: " + args.EventMessage);
			_eventHandled = true;
		}

		public bool EventHandled
		{
			get { return _eventHandled; }
		}

		public void NeverCall()
		{
			throw new Exception();
		}

		public string FakeEventHandler(object sender, MyClientEventArgs args)
		{
			throw new Exception();
		}
	}
}
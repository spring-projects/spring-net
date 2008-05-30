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

namespace Spring.IocQuickStart.EventRegistry
{
	public delegate void SimpleClientEvent(object sender, MyClientEventArgs args);

	public class MyEventPublisher
	{
		private string _publisherName;

		public event SimpleClientEvent MyClientEvent1;

		public MyEventPublisher()
		{
		}

		public string PublisherName
		{
			get { return _publisherName; }
			set { _publisherName = value; }
		}

		public void ClientMethodThatTriggersEvent1()
		{
			if (MyClientEvent1 != null)
			{
				MyClientEvent1(this, new MyClientEventArgs("Event 1 raised from " + _publisherName));
			}
		}
	}
}